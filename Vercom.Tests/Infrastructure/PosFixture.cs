using Microsoft.EntityFrameworkCore;
using VERCOM.Models;

namespace Vercom.Tests.Infrastructure;

/// <summary>
/// Construye la jerarquía mínima del POS —caja, dispositivo y sesión— dentro de
/// la transacción de un test, para que se revierta al terminar.
/// Los datos base (sucursal, almacén, cuenta contable y usuario) se toman de la
/// BD sembrada porque las claves foráneas del esquema son reales: no se pueden
/// inventar GUIDs.
/// </summary>
public static class PosFixture
{
    public sealed record SesionPos(Guid SesionId, Guid CajaId, Guid DispositivoPosId, Guid EntidadId, Guid CajeroId);

    public static async Task<SesionPos> CrearSesionAsync(
        AppDbContext db,
        string estado = "ABIERTA",
        decimal montoApertura = 0m,
        decimal totalEfectivo = 0m)
    {
        var sucursal = await db.Sucursals.AsNoTracking().OrderBy(s => s.Codigo).FirstAsync();
        var almacen = await db.Almacens.AsNoTracking().OrderBy(a => a.Codigo).FirstAsync();
        var cuenta = await db.CuentaContables.AsNoTracking().OrderBy(c => c.Codigo).FirstAsync();
        var cajero = await db.Usuarios.AsNoTracking().OrderBy(u => u.NombreUsuario).FirstAsync();

        var caja = new Caja
        {
            Id = Guid.NewGuid(),
            EntidadId = sucursal.EntidadId,
            SucursalId = sucursal.Id,
            Nombre = "CAJA FIXTURE",
            CuentaContableId = cuenta.Id,
            SaldoActual = 0m,
            Activa = true
        };

        var dispositivo = new DispositivoPo
        {
            Id = Guid.NewGuid(),
            EntidadId = sucursal.EntidadId,
            SucursalId = sucursal.Id,
            AlmacenId = almacen.Id,
            CajaId = caja.Id,
            Codigo = "FIXTURE-POS",
            Nombre = "Dispositivo de prueba",
            Estado = "ACTIVO",
            CreadoEn = DateTimeOffset.UtcNow
        };

        var sesion = new SesionCajaPo
        {
            Id = Guid.NewGuid(),
            CajaId = caja.Id,
            DispositivoPosId = dispositivo.Id,
            CajeroId = cajero.Id,
            FechaApertura = DateTimeOffset.UtcNow,
            MontoApertura = montoApertura,
            TotalVentas = 0m,
            TotalEfectivo = totalEfectivo,
            TotalTransfermovil = 0m,
            TotalEnzona = 0m,
            TotalOtrosMedios = 0m,
            CantidadFacturas = 0,
            Estado = estado
        };

        db.Cajas.Add(caja);
        db.DispositivoPos.Add(dispositivo);
        db.SesionCajaPos.Add(sesion);
        await db.SaveChangesAsync();

        return new SesionPos(sesion.Id, caja.Id, dispositivo.Id, caja.EntidadId, cajero.Id);
    }

    public static async Task AgregarVentaPendienteAsync(AppDbContext db, SesionPos sesion)
    {
        db.PosVentaPendientes.Add(new PosVentaPendiente
        {
            Id = Guid.NewGuid(),
            DispositivoPosId = sesion.DispositivoPosId,
            SesionCajaPosId = sesion.SesionId,
            IdempotencyKey = $"test-{Guid.NewGuid():N}",
            PayloadJson = "{}",
            FechaVentaLocal = DateTimeOffset.UtcNow,
            FechaRecibidoServidor = DateTimeOffset.UtcNow,
            Estado = "PENDIENTE",
            IntentosProcesamiento = 0
        });

        await db.SaveChangesAsync();
    }
}
