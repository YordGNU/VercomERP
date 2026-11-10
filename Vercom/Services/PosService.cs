using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VERCOM.DTOs;
using VERCOM.Models;
using VERCOM.ViewModels;

namespace VERCOM.Services;

public interface IPosService
{
    // Dispositivos
    Task<IEnumerable<DispositivoPo>> GetDevicesAsync();
    Task<DispositivoPo?> GetDeviceByIdAsync(Guid id);
    Task<DispositivoPosFormViewModel> GetDeviceFormContextAsync(DispositivoPo? existing = null);
    Task<(IEnumerable<SelectListItem> Almacenes, IEnumerable<SelectListItem> Cajas)> GetUbicacionesPorSucursalAsync(Guid sucursalId);
    Task<(bool Succeeded, string Message)> SaveDeviceAsync(DispositivoPo device, Guid? entidadId = null);
    Task<(bool Succeeded, string? Message)> HeartbeatAsync(Guid dispositivoPosId);

    // Sesiones
    Task<IEnumerable<SesionCajaPo>> GetSessionsAsync();
    Task<SesionCajaPo?> GetSessionByIdAsync(Guid id);
    Task<(bool Succeeded, string Message)> CloseSessionAsync(Guid sessionId, decimal declaredAmount, string notes, Guid supervisorId);

    // Ventas Pendientes
    Task<IEnumerable<PosVentaPendiente>> GetPendingSalesAsync();
    Task<PosVentaPendiente?> GetPendingSaleByIdAsync(Guid id);
    Task<(bool Succeeded, string Message)> ProcessPendingSaleAsync(Guid pendingId);

    // Numeración
    Task<IEnumerable<PosRangoNumeracion>> GetRangesAsync();

    // Sincronización de Datos (Master Data para POS)
    Task<IReadOnlyList<PosCatalogoProductoDto>> GetCatalogForPosAsync(DateTimeOffset? updatedSince, Guid? clienteId = null);
}

public class PosService : IPosService
{
    private readonly AppDbContext _context;
    private readonly Security.IEntidadProvider _entidadProvider;
    private readonly IConsecutivoService _consecutivoService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly INotificationService _notifications;
    private readonly ArqueoCajaContableService _arqueo;
    private readonly IPricingService _pricingService;

    public PosService(AppDbContext context, Security.IEntidadProvider entidadProvider, IConsecutivoService consecutivoService, IServiceScopeFactory serviceScopeFactory, INotificationService notifications, ArqueoCajaContableService arqueo, IPricingService pricingService)
    {
        _context = context;
        _entidadProvider = entidadProvider;
        _consecutivoService = consecutivoService;
        _serviceScopeFactory = serviceScopeFactory;
        _notifications = notifications;
        _arqueo = arqueo;
        _pricingService = pricingService;
    }

    public async Task<IEnumerable<DispositivoPo>> GetDevicesAsync()
    {
        var entidadId = EntidadId();
        IQueryable<DispositivoPo> query = _context.DispositivoPos.Include(d => d.Sucursal);
        if (entidadId.HasValue) query = query.Where(d => d.EntidadId == entidadId.Value);
        return await query.ToListAsync();
    }

    public async Task<DispositivoPo?> GetDeviceByIdAsync(Guid id)
    {
        return await _context.DispositivoPos.Include(d => d.Sucursal).Include(d => d.Caja).FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<DispositivoPosFormViewModel> GetDeviceFormContextAsync(DispositivoPo? existing = null)
    {
        // La entidad no se elige: sale del usuario (o de la sesión, en MASTER). La cascada es
        // sucursal -> cajas/almacenes, y para un usuario con sucursal asignada esa es la suya.
        var entidad = existing is { EntidadId: var e } && e != Guid.Empty ? e : _entidadProvider.CurrentEntidadId;
        var sucursalUsuario = _entidadProvider.IsMaster ? null : _entidadProvider.CurrentSucursalId;
        var sucursal = existing is { SucursalId: var s } && s != Guid.Empty ? s : sucursalUsuario;

        var sucursales = await _context.Sucursals
            .Where(x => x.Activo && x.EntidadId == entidad)
            .Where(x => sucursalUsuario == null || x.Id == sucursalUsuario || x.Id == sucursal)
            .OrderBy(x => x.Nombre)
            .Select(x => new SelectListItem(x.Nombre, x.Id.ToString()))
            .ToListAsync();

        var ubicaciones = sucursal.HasValue
            ? await GetUbicacionesPorSucursalAsync(sucursal.Value)
            : (Almacenes: (IEnumerable<SelectListItem>)new List<SelectListItem>(),
               Cajas: (IEnumerable<SelectListItem>)new List<SelectListItem>());

        return new DispositivoPosFormViewModel
        {
            Dispositivo = existing ?? new DispositivoPo
            {
                Estado = "ACTIVO",
                EntidadId = entidad,
                SucursalId = sucursalUsuario ?? Guid.Empty
            },
            EntidadNombre = entidad == Guid.Empty
                ? null
                : await _context.Entidads.Where(x => x.Id == entidad)
                    .Select(x => x.NombreComercial ?? x.RazonSocial).FirstOrDefaultAsync(),
            Sucursales = sucursales,
            Almacenes = ubicaciones.Almacenes,
            Cajas = ubicaciones.Cajas
        };
    }

    // Cajas y almacenes de una sucursal concreta: alimenta la carga dinámica del formulario.
    public async Task<(IEnumerable<SelectListItem> Almacenes, IEnumerable<SelectListItem> Cajas)> GetUbicacionesPorSucursalAsync(Guid sucursalId)
    {
        if (sucursalId == Guid.Empty)
            return (new List<SelectListItem>(), new List<SelectListItem>());

        var almacenes = await _context.Almacens.Where(a => a.Activo && a.SucursalId == sucursalId)
            .OrderBy(a => a.Nombre).Select(a => new SelectListItem(a.Nombre, a.Id.ToString())).ToListAsync();
        var cajas = await _context.Cajas.Where(c => c.Activa && c.SucursalId == sucursalId)
            .OrderBy(c => c.Nombre).Select(c => new SelectListItem(c.Nombre, c.Id.ToString())).ToListAsync();

        return (almacenes, cajas);
    }

    public async Task<(bool Succeeded, string Message)> SaveDeviceAsync(DispositivoPo device, Guid? entidadId = null)
    {
        try
        {
            var entidad = entidadId is { } id && id != Guid.Empty ? id : _entidadProvider.CurrentEntidadId;
            if (entidad == Guid.Empty)
                return (false, "El usuario no tiene una entidad asignada; no se puede registrar el terminal.");

            if (device.SucursalId == Guid.Empty || device.AlmacenId == Guid.Empty || device.CajaId == Guid.Empty)
                return (false, "Seleccione sucursal, almacén y caja del terminal.");

            var sucursal = await _context.Sucursals.FirstOrDefaultAsync(s => s.Id == device.SucursalId && s.EntidadId == entidad);
            if (sucursal == null)
                return (false, "La sucursal seleccionada no pertenece a la entidad del usuario.");

            var sucursalUsuario = _entidadProvider.IsMaster ? null : _entidadProvider.CurrentSucursalId;
            if (sucursalUsuario.HasValue && sucursalUsuario.Value != device.SucursalId)
                return (false, "Solo puede registrar terminales en su sucursal asignada.");

            if (!await _context.Almacens.AnyAsync(a => a.Id == device.AlmacenId && a.SucursalId == device.SucursalId))
                return (false, "El almacén seleccionado no pertenece a la sucursal del terminal.");
            if (!await _context.Cajas.AnyAsync(c => c.Id == device.CajaId && c.SucursalId == device.SucursalId))
                return (false, "La caja seleccionada no pertenece a la sucursal del terminal.");

            if (device.Id == Guid.Empty)
            {
                device.Id = Guid.NewGuid();
                device.EntidadId = entidad;
                device.CreadoEn = DateTimeOffset.UtcNow;
                _context.DispositivoPos.Add(device);
            }
            else
            {
                var existing = await _context.DispositivoPos.FindAsync(device.Id);
                if (existing == null) return (false, "No existe.");
                _context.Entry(existing).CurrentValues.SetValues(device);
                existing.EntidadId = entidad;
            }
            await _context.SaveChangesAsync();
            return (true, "Dispositivo guardado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string? Message)> HeartbeatAsync(Guid dispositivoPosId)
    {
        if (dispositivoPosId == Guid.Empty) return (false, "Dispositivo requerido.");
        var device = await _context.DispositivoPos.FirstOrDefaultAsync(d => d.Id == dispositivoPosId);
        if (device is null || device.Estado != "ACTIVO") return (false, "Dispositivo no encontrado o inactivo.");

        // Transición OFFLINE -> ONLINE: se notifica cuando convive <30min (o nunca sincronizó),
        // evitando repetir el aviso en cada heartbeat periódico (~15 min).
        var wasOnline = device.UltimaSincronizacion is not null &&
            device.UltimaSincronizacion >= DateTimeOffset.UtcNow.AddMinutes(-30);

        device.UltimaSincronizacion = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();

        if (!wasOnline)
        {
            await _notifications.NotifyEntityAsync(device.EntidadId, "Terminal POS en línea",
                $"{device.Nombre} ({device.Codigo}) volvió a conectarse.", "success");
        }

        return (true, null);
    }

    public async Task<IEnumerable<SesionCajaPo>> GetSessionsAsync()
    {
        var entidadId = EntidadId();
        IQueryable<SesionCajaPo> query = _context.SesionCajaPos.Include(s => s.DispositivoPos);
        if (entidadId.HasValue) query = query.Where(s => s.DispositivoPos.EntidadId == entidadId.Value);
        return await query.OrderByDescending(s => s.FechaApertura).ToListAsync();
    }

    public async Task<SesionCajaPo?> GetSessionByIdAsync(Guid id)
    {
        return await _context.SesionCajaPos
            .Include(s => s.DispositivoPos)
            .Include(s => s.FacturaVenta)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<(bool Succeeded, string Message)> CloseSessionAsync(Guid sessionId, decimal declaredAmount, string notes, Guid supervisorId)
    {
        if (declaredAmount < 0) return (false, "El monto de cierre no puede ser negativo.");
        var session = await _context.SesionCajaPos
            .Include(s => s.DispositivoPos)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session == null || session.Estado != "ABIERTA") return (false, "Sesión no válida.");

        // El arqueo de sistema es MONTO DE APERTURA + EFECTIVO VENDIDO (mismo criterio del API de cierre POS).
        session.MontoCierreSistema = session.MontoApertura + session.TotalEfectivo;
        session.FechaCierre = DateTimeOffset.UtcNow;
        session.MontoCierreDeclarado = declaredAmount;
        session.ObservacionesCierre = notes;
        session.SupervisorConciliacionId = supervisorId;

        var diferencia = (declaredAmount - (session.MontoCierreSistema ?? 0m));
        if (diferencia != 0)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var res = await _arqueo.ContabilizarAsync(session.Id, session.CajaId, session.CajeroId, diferencia, session.FechaCierre, default);
                if (!res.Succeeded || res.Entry is null)
                {
                    await transaction.RollbackAsync();
                    return (false, "No se pudo contabilizar el arqueo: revise la configuración de cuentas contables.");
                }
                session.AsientoCierreId = res.Entry.Id;
                session.Estado = "CERRADA";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                return (false, "No se pudo contabilizar el arqueo: revise la configuración de cuentas contables.");
            }
        }
        else
        {
            session.Estado = "CERRADA";
            await _context.SaveChangesAsync();
        }

        await _notifications.NotifyEntityAsync(session.DispositivoPos.EntidadId, "Sesión de caja cerrada",
            $"{session.DispositivoPos.Nombre} cerró la sesión: {session.TotalVentas:N2} en ventas, {session.CantidadFacturas} facturas, diferencia de arqueo {session.DiferenciaArqueo:N2}.", "info");

        return (true, "Sesión cerrada y conciliada.");
    }

    public async Task<IEnumerable<PosVentaPendiente>> GetPendingSalesAsync()
    {
        var entidadId = EntidadId();
        IQueryable<PosVentaPendiente> query = _context.PosVentaPendientes.Include(p => p.DispositivoPos);
        if (entidadId.HasValue) query = query.Where(p => p.DispositivoPos.EntidadId == entidadId.Value);
        return await query.OrderByDescending(p => p.FechaRecibidoServidor).ToListAsync();
    }

    public async Task<PosVentaPendiente?> GetPendingSaleByIdAsync(Guid id)
    {
        return await _context.PosVentaPendientes.Include(p => p.DispositivoPos).FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<(bool Succeeded, string Message)> ProcessPendingSaleAsync(Guid pendingId)
    {
        var pending = await _context.PosVentaPendientes.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pendingId);
        if (pending is null) return (false, "Venta pendiente no encontrada.");

        if (pending.Estado == "PROCESADO") return (true, "La venta ya estaba procesada.");
        if (pending.Estado != "PENDIENTE") return (false, $"La venta está en estado {pending.Estado} y no se puede procesar.");

        // Se procesa de forma síncrona reutilizando el mismo flujo transaccional que la API
        // JWT (valida sesión, existencias, crea factura, mueve inventario y contabiliza).
        // Un payload de un formato anterior al actual no se procesa: ProcesarAsync lo rechaza
        // con el motivo concreto en vez de encolarlo a un procesador que ya no existe.
        using var scope = _serviceScopeFactory.CreateScope();
        var sync = scope.ServiceProvider.GetRequiredService<PosSincronizacionService>();
        var result = await sync.ProcesarAsync(pending.DispositivoPosId, pending.IdempotencyKey, CancellationToken.None);
        if (result.Error is not null) return (false, result.Error);
        return result.Result!.Estado == "PROCESADO"
            ? (true, "Venta procesada correctamente.")
            : (true, "La venta quedó pendiente.");
    }

    public async Task<IReadOnlyList<PosCatalogoProductoDto>> GetCatalogForPosAsync(DateTimeOffset? updatedSince, Guid? clienteId = null)
    {
        var entidadId = EntidadId();
        if (!entidadId.HasValue)
        {
            entidadId = await _context.Productos.AsNoTracking()
                .Select(p => (Guid?)p.EntidadId)
                .FirstOrDefaultAsync() ?? Guid.Empty;
        }

        var query = _context.Productos.AsNoTracking();

        if (updatedSince.HasValue)
        {
            query = query.Where(p => p.ActualizadoEn > updatedSince.Value);
        }

        var items = await query.Select(p => new
        {
            serverId = p.Id,
            cod = p.Codigo,
            nombre = p.Nombre,
            precioVentaBase = p.PrecioVentaActual ?? 0m,
            unidad = p.UnidadMedida.Codigo,
            existencias = _context.Existencia.Where(e => e.ProductoId == p.Id).Sum(e => e.Cantidad)
        }).ToListAsync();

        var fecha = DateOnly.FromDateTime(DateTime.Now);
        var precios = await _pricingService.GetPriceMapAsync(entidadId.Value, clienteId, fecha);
        var porProducto = precios.ToDictionary(kv => kv.Key, kv => kv.Value);

        return items.Select(p =>
        {
            porProducto.TryGetValue(p.serverId, out var resuelto);
            return new PosCatalogoProductoDto
            {
                serverId = p.serverId,
                cod = p.cod,
                nombre = p.nombre,
                precio = resuelto != null ? resuelto.Precio : p.precioVentaBase,
                origenPrecio = resuelto?.SourceName ?? (p.precioVentaBase > 0m ? "PLANO" : "SIN_PRECIO"),
                unidad = p.unidad,
                existencias = p.existencias
            };
        }).ToList();
    }

    public async Task<IEnumerable<PosRangoNumeracion>> GetRangesAsync()
    {
        var entidadId = EntidadId();
        IQueryable<PosRangoNumeracion> query = _context.PosRangoNumeracions.Include(r => r.DispositivoPos);
        if (entidadId.HasValue) query = query.Where(r => r.DispositivoPos.EntidadId == entidadId.Value);
        return await query.OrderByDescending(r => r.AsignadoEn).Take(200).ToListAsync();
    }

    // Filtro multi-tenant: MASTER (o sin claim de entidad) ve todo; el resto solo su entidad.
    private Guid? EntidadId()
    {
        if (_entidadProvider.IsMaster) return null;
        var id = _entidadProvider.CurrentEntidadId;
        return id == Guid.Empty ? null : id;
    }
}
