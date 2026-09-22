using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface IPosService
{
    // Dispositivos
    Task<IEnumerable<DispositivoPo>> GetDevicesAsync();
    Task<DispositivoPo?> GetDeviceByIdAsync(Guid id);
    Task<DispositivoPosFormViewModel> GetDeviceFormContextAsync(DispositivoPo? existing = null);
    Task<(bool Succeeded, string Message)> SaveDeviceAsync(DispositivoPo device);
    Task<(bool Succeeded, string? Message)> HeartbeatAsync(Guid dispositivoPosId);

    // Sesiones
    Task<IEnumerable<SesionCajaPo>> GetSessionsAsync();
    Task<SesionCajaPo?> GetSessionByIdAsync(Guid id);
    Task<(bool Succeeded, string Message)> CloseSessionAsync(Guid sessionId, decimal declaredAmount, string notes, Guid supervisorId);

    // Ventas Pendientes
    Task<IEnumerable<PosVentaPendiente>> GetPendingSalesAsync();
    Task<PosVentaPendiente?> GetPendingSaleByIdAsync(Guid id);
    Task<(bool Succeeded, string Message)> ProcessPendingSaleAsync(Guid pendingId);
    Task<(bool Succeeded, string Message)> QueuePendingSalesAsync(List<Vercom.DTOs.OperacionRequestDto> operaciones);

    // Numeración
    Task<(string Serie, long Desde, long Hasta)> ReserveNumberRangeAsync(Guid dispositivoId, string serie, int cantidad);
    Task<IEnumerable<PosRangoNumeracion>> GetRangesAsync();

    // Sincronización de Datos (Master Data para POS)
    Task<dynamic> GetCatalogForPosAsync(DateTimeOffset? updatedSince);
}

public class PosService : IPosService
{
    private readonly AppDbContext _context;
    private readonly Security.IEntidadProvider _entidadProvider;
    private readonly IConsecutivoService _consecutivoService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly INotificationService _notifications;

    public PosService(AppDbContext context, Security.IEntidadProvider entidadProvider, IConsecutivoService consecutivoService, IServiceScopeFactory serviceScopeFactory, INotificationService notifications)
    {
        _context = context;
        _entidadProvider = entidadProvider;
        _consecutivoService = consecutivoService;
        _serviceScopeFactory = serviceScopeFactory;
        _notifications = notifications;
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
        var entidadId = EntidadId();
        IQueryable<Sucursal> sucursales = _context.Sucursals.Where(s => s.Activo);
        IQueryable<Almacen> almacenes = _context.Almacens.Where(a => a.Activo);
        IQueryable<Caja> cajas = _context.Cajas.Where(c => c.Activa);
        if (entidadId.HasValue)
        {
            sucursales = sucursales.Where(s => s.EntidadId == entidadId.Value);
            almacenes = almacenes.Where(a => a.EntidadId == entidadId.Value);
            cajas = cajas.Where(c => c.EntidadId == entidadId.Value);
        }

        return new DispositivoPosFormViewModel
        {
            Dispositivo = existing ?? new DispositivoPo { Estado = "ACTIVO" },
            Sucursales = new SelectList(await sucursales.OrderBy(s => s.Nombre).ToListAsync(), "Id", "Nombre"),
            Almacenes = new SelectList(await almacenes.OrderBy(a => a.Nombre).ToListAsync(), "Id", "Nombre"),
            Cajas = new SelectList(await cajas.OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre")
        };
    }

    public async Task<(bool Succeeded, string Message)> SaveDeviceAsync(DispositivoPo device)
    {
        try
        {
            if (device.Id == Guid.Empty)
            {
                device.Id = Guid.NewGuid();
                device.EntidadId = _entidadProvider.CurrentEntidadId;
                device.CreadoEn = DateTimeOffset.UtcNow;
                _context.DispositivoPos.Add(device);
            }
            else
            {
                var existing = await _context.DispositivoPos.FindAsync(device.Id);
                if (existing == null) return (false, "No existe.");
                _context.Entry(existing).CurrentValues.SetValues(device);
                existing.EntidadId = _entidadProvider.CurrentEntidadId;
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
        session.DiferenciaArqueo = declaredAmount - session.MontoCierreSistema;
        session.ObservacionesCierre = notes;
        session.SupervisorConciliacionId = supervisorId;
        session.Estado = "CERRADA";

        await _context.SaveChangesAsync();

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

        // Formato nuevo (VentaPosPayload): procesar de forma síncrona reutilizando el flujo
        // transaccional del API (valida sesión, existencias, crea factura y mueve inventario).
        if (pending.PayloadJson.IndexOf("TipoMovimientoId", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var sync = scope.ServiceProvider.GetRequiredService<PosSincronizacionService>();
            var result = await sync.ProcesarAsync(pending.DispositivoPosId, pending.IdempotencyKey, CancellationToken.None);
            if (result.Error is not null) return (false, result.Error);
            return result.Result!.Estado == "PROCESADO"
                ? (true, "Venta procesada correctamente.")
                : (true, "La venta quedó pendiente.");
        }

        // Formato legacy: lo toma el worker de sincronización en el siguiente barrido (hasta 30s).
        return (true, "Venta legacy encargada al procesador en segundo plano (hasta 30 segundos).");
    }

    public async Task<(bool Succeeded, string Message)> QueuePendingSalesAsync(List<Vercom.DTOs.OperacionRequestDto> operaciones)
    {
        try
        {
            foreach (var op in operaciones)
            {
                var deviceId = Guid.Parse(op.TerminalId ?? Guid.Empty.ToString());
                var existe = await _context.PosVentaPendientes
                    .AnyAsync(p => p.IdempotencyKey == op.LocalId.ToString() && p.DispositivoPosId == deviceId);

                if (existe) continue;

                var pendiente = new PosVentaPendiente
                {
                    Id = Guid.NewGuid(),
                    DispositivoPosId = deviceId,
                    IdempotencyKey = op.LocalId.ToString(),
                    PayloadJson = System.Text.Json.JsonSerializer.Serialize(op),
                    Estado = "PENDIENTE",
                    FechaRecibidoServidor = DateTimeOffset.Now,
                    FechaVentaLocal = op.Fecha ?? DateTimeOffset.Now
                };

                _context.PosVentaPendientes.Add(pendiente);
            }

            await _context.SaveChangesAsync();
            return (true, "Operaciones encoladas correctamente.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(string Serie, long Desde, long Hasta)> ReserveNumberRangeAsync(Guid dispositivoId, string serie, int cantidad)
    {
        var dispositivo = await _context.DispositivoPos.FindAsync(dispositivoId);
        if (dispositivo == null) throw new Exception("Dispositivo no registrado");
        if (cantidad <= 0 || cantidad > 100000) throw new Exception("La cantidad reservada debe estar entre 1 y 100000.");

        // Reserva atómica: UPDLOCK en la misma transacción garantiza exclusión mutua
        // entre dispositivos de la sucursal y evita reservas duplicadas.
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var sql = @"
            SELECT * FROM nucleo.consecutivo WITH (UPDLOCK, ROWLOCK)
            WHERE entidad_id = {0}
            AND (sucursal_id = {1} OR ({1} IS NULL AND sucursal_id IS NULL))
            AND tipo_documento = {2}
            AND serie = {3}";

        var consecutivo = await _context.Consecutivos
            .FromSqlRaw(sql, dispositivo.EntidadId, (object?)dispositivo.SucursalId ?? DBNull.Value, Vercom.Helpers.DocumentoTipo.FacturaVenta, serie)
            .FirstOrDefaultAsync();

        long start;
        if (consecutivo == null)
        {
            consecutivo = new Consecutivo
            {
                EntidadId = dispositivo.EntidadId,
                SucursalId = dispositivo.SucursalId,
                TipoDocumento = Vercom.Helpers.DocumentoTipo.FacturaVenta,
                Serie = serie,
                UltimoNumero = 1,
                LongitudPadding = 8,
                ActualizadoEn = DateTimeOffset.Now
            };
            _context.Consecutivos.Add(consecutivo);
            start = 1;
        }
        else
        {
            start = consecutivo.UltimoNumero + 1;
        }

        var end = start + cantidad - 1;
        consecutivo.UltimoNumero = end;
        consecutivo.ActualizadoEn = DateTimeOffset.Now;

        var reserva = new PosRangoNumeracion
        {
            Id = Guid.NewGuid(),
            DispositivoPosId = dispositivoId,
            Serie = serie,
            NumeroDesde = (int)start,
            NumeroHasta = (int)end,
            NumeroSiguienteLocal = (int)start,
            AsignadoEn = DateTimeOffset.Now,
            Agotado = false
        };

        _context.PosRangoNumeracions.Add(reserva);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return (serie, start, end);
    }

    public async Task<dynamic> GetCatalogForPosAsync(DateTimeOffset? updatedSince)
    {
        var query = _context.Productos.Include(p => p.UnidadMedida).AsQueryable();

        if (updatedSince.HasValue)
        {
            query = query.Where(p => p.ActualizadoEn > updatedSince.Value);
        }

        var res = await query.Select(p => new
        {
            serverId = p.Id,
            cod = p.Codigo,
            nombre = p.Nombre,
            precio = p.PrecioVentaActual,
            unidad = p.UnidadMedida.Codigo,
            existencias = _context.Existencia.Where(e => e.ProductoId == p.Id).Sum(e => e.Cantidad)
        }).ToListAsync();

        return res;
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
