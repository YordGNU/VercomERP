using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

    // Sesiones
    Task<IEnumerable<SesionCajaPo>> GetSessionsAsync();
    Task<SesionCajaPo?> GetSessionByIdAsync(Guid id);
    Task<(bool Succeeded, string Message)> CloseSessionAsync(Guid sessionId, decimal declaredAmount, string notes, Guid supervisorId);

    // Ventas Pendientes
    Task<IEnumerable<PosVentaPendiente>> GetPendingSalesAsync();
    Task<(bool Succeeded, string Message)> ProcessPendingSaleAsync(Guid pendingId);
    Task<(bool Succeeded, string Message)> QueuePendingSalesAsync(List<Vercom.DTOs.OperacionRequestDto> operaciones);

    // Numeración
    Task<(string Serie, long Desde, long Hasta)> ReserveNumberRangeAsync(Guid dispositivoId, string serie, int cantidad);

    // Sincronización de Datos (Master Data para POS)
    Task<dynamic> GetCatalogForPosAsync(DateTimeOffset? updatedSince);
}

public class PosService : IPosService
{
    private readonly AppDbContext _context;
    private readonly Security.IEntidadProvider _entidadProvider;
    private readonly IConsecutivoService _consecutivoService;

    public PosService(AppDbContext context, Security.IEntidadProvider entidadProvider, IConsecutivoService consecutivoService)
    {
        _context = context;
        _entidadProvider = entidadProvider;
        _consecutivoService = consecutivoService;
    }

    public async Task<IEnumerable<DispositivoPo>> GetDevicesAsync()
    {
        return await _context.DispositivoPos.Include(d => d.Sucursal).ToListAsync();
    }

    public async Task<DispositivoPo?> GetDeviceByIdAsync(Guid id)
    {
        return await _context.DispositivoPos.FindAsync(id);
    }

    public async Task<DispositivoPosFormViewModel> GetDeviceFormContextAsync(DispositivoPo? existing = null)
    {
        return new DispositivoPosFormViewModel
        {
            Dispositivo = existing ?? new DispositivoPo { Estado = "ONLINE" },
            Sucursales = new SelectList(await _context.Sucursals.Where(s => s.Activo).ToListAsync(), "Id", "Nombre")
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

    public async Task<IEnumerable<SesionCajaPo>> GetSessionsAsync()
    {
        return await _context.SesionCajaPos.Include(s => s.DispositivoPos).OrderByDescending(s => s.FechaApertura).ToListAsync();
    }

    public async Task<SesionCajaPo?> GetSessionByIdAsync(Guid id)
    {
        return await _context.SesionCajaPos.Include(s => s.DispositivoPos).FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<(bool Succeeded, string Message)> CloseSessionAsync(Guid sessionId, decimal declaredAmount, string notes, Guid supervisorId)
    {
        var session = await _context.SesionCajaPos.FindAsync(sessionId);
        if (session == null || session.Estado != "ABIERTA") return (false, "Sesión no válida.");

        session.FechaCierre = DateTimeOffset.Now;
        session.MontoCierreDeclarado = declaredAmount;
        session.DiferenciaArqueo = declaredAmount - session.MontoCierreSistema;
        session.ObservacionesCierre = notes;
        session.SupervisorConciliacionId = supervisorId;
        session.Estado = "CERRADA";

        await _context.SaveChangesAsync();
        return (true, "Sesión cerrada y conciliada.");
    }

    public async Task<IEnumerable<PosVentaPendiente>> GetPendingSalesAsync()
    {
        return await _context.PosVentaPendientes.Include(p => p.DispositivoPos).Where(p => p.Estado == "PENDIENTE").ToListAsync();
    }

    public async Task<(bool Succeeded, string Message)> ProcessPendingSaleAsync(Guid pendingId)
    {
        // Lógica de integración asíncrona
        return (true, "Procesamiento disparado.");
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

        var primerNumeroStr = await _consecutivoService.ObtenerSiguienteNumeroAsync(
            dispositivo.EntidadId, dispositivo.SucursalId, "FACTURA_VENTA", serie);

        long start = long.Parse(primerNumeroStr);
        long end = start + cantidad - 1;

        // Actualizar el contador del sistema para saltar el bloque reservado
        var config = await _context.Consecutivos.FirstAsync(c =>
            c.EntidadId == dispositivo.EntidadId && c.Serie == serie && c.TipoDocumento == "FACTURA_VENTA");

        config.UltimoNumero = end;

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
}
