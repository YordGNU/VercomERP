using Microsoft.EntityFrameworkCore;
using Vercom.DTOs;
using Vercom.Models;

namespace Vercom.Services;

public sealed class PosCajaService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public PosCajaService(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<(SesionCajaPosDto? Sesion, string? Error, bool Conflict)> AbrirAsync(AbrirSesionCajaPosRequest request, CancellationToken cancellationToken)
    {
        if (request.DispositivoPosId == Guid.Empty || request.CajeroId == Guid.Empty || request.MontoApertura < 0)
            return (null, "DispositivoPosId, CajeroId y monto de apertura son obligatorios.", false);

        var device = await _db.DispositivoPos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.DispositivoPosId && (x.Estado == "ACTIVO" || x.Estado == "ONLINE"), cancellationToken);
        if (device is null) return (null, "Dispositivo POS no encontrado o inactivo.", false);

        // La sesión es compartida por todos los dispositivos de la misma caja: si ya hay
        // una ABIERTA para esa caja, se devuelve (aunque la haya abierto otro dispositivo).
        var open = await _db.SesionCajaPos.FirstOrDefaultAsync(x => x.CajaId == device.CajaId && x.Estado == "ABIERTA", cancellationToken);
        if (open is not null) return (Map(open), null, true);

        var session = new SesionCajaPo
        {
            Id = Guid.NewGuid(),
            CajaId = device.CajaId,
            DispositivoPosId = request.DispositivoPosId,
            CajeroId = request.CajeroId,
            FechaApertura = DateTimeOffset.UtcNow,
            MontoApertura = request.MontoApertura,
            TotalVentas = 0,
            TotalEfectivo = 0,
            TotalTransfermovil = 0,
            TotalEnzona = 0,
            TotalOtrosMedios = 0,
            CantidadFacturas = 0,
            Estado = "ABIERTA"
        };
        _db.SesionCajaPos.Add(session);
        try { await _db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException)
        {
            // Dos peticiones concurrentes pueden violar el índice único de sesión abierta;
            // se resuelve devolviendo la sesión existente en vez de un 500.
            var existing = await _db.SesionCajaPos.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CajaId == device.CajaId && x.Estado == "ABIERTA", cancellationToken);
            if (existing is not null) return (Map(existing), null, true);
            throw;
        }

        await _notifications.NotifyEntityAsync(device.EntidadId, "Sesión de caja abierta",
            $"{device.Nombre} abrió una sesión POS con {request.MontoApertura:N2} de apertura.", "success");

        return (Map(session), null, false);
    }

    public async Task<SesionCajaPosDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var session = await _db.SesionCajaPos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return session is null ? null : Map(session);
    }

    public async Task<SesionCajaPosDto?> GetActualAsync(Guid dispositivoPosId, CancellationToken cancellationToken)
    {
        if (dispositivoPosId == Guid.Empty) return null;
        var device = await _db.DispositivoPos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dispositivoPosId, cancellationToken);
        if (device is null) return null;
        var session = await _db.SesionCajaPos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CajaId == device.CajaId && x.Estado == "ABIERTA", cancellationToken);
        return session is null ? null : Map(session);
    }

    public async Task<(SesionCajaPosDto? Sesion, string? Error, bool Conflict)> CerrarAsync(Guid id, CerrarSesionCajaPosRequest request, CancellationToken cancellationToken)
    {
        if (request.MontoCierreDeclarado < 0) return (null, "El monto de cierre no puede ser negativo.", false);
        var session = await _db.SesionCajaPos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (session is null) return (null, null, false);
        if (session.Estado != "ABIERTA") return (Map(session), null, true);
        var pendientes = await _db.PosVentaPendientes.AnyAsync(x => x.SesionCajaPosId == session.Id && x.Estado == "PENDIENTE", cancellationToken);
        if (pendientes) return (null, "No se puede cerrar la caja con ventas pendientes de procesar.", true);
        session.FechaCierre = DateTimeOffset.UtcNow;
        session.MontoCierreDeclarado = request.MontoCierreDeclarado;
        session.MontoCierreSistema = session.MontoApertura + session.TotalEfectivo;
        session.DiferenciaArqueo = request.MontoCierreDeclarado - session.MontoCierreSistema;
        session.Estado = "CERRADA";
        await _db.SaveChangesAsync(cancellationToken);
        return (Map(session), null, false);
    }

    private static SesionCajaPosDto Map(SesionCajaPo x) => new()
    {
        Id = x.Id, CajaId = x.CajaId, DispositivoPosId = x.DispositivoPosId, CajeroId = x.CajeroId, FechaApertura = x.FechaApertura,
        FechaCierre = x.FechaCierre, MontoApertura = x.MontoApertura, MontoCierreDeclarado = x.MontoCierreDeclarado,
        MontoCierreSistema = x.MontoCierreSistema, TotalVentas = x.TotalVentas, TotalEfectivo = x.TotalEfectivo,
        CantidadFacturas = x.CantidadFacturas, Estado = x.Estado
    };
}