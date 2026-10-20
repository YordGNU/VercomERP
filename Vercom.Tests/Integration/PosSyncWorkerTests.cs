using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Vercom.Tests.Infrastructure;
using VERCOM.Models;
using VERCOM.Security;
using VERCOM.Services;
using Xunit;

namespace Vercom.Tests.Integration;

/// <summary>
/// Barrido del worker de sincronización POS: una fila irrecuperable —numeración ya usada por
/// otra venta del mismo barrido, índice único (entidad, sucursal, serie, número)— no puede
/// bloquear la cola ni dejar el pendiente marcado PROCESADO con una factura que no existe.
/// </summary>
public class PosSyncWorkerTests
{
    [DbFact(RequiereDatosBase = true)]
    public async Task UnaFilaVenenosaNoBloqueaElBarridoNiEnsuciaElPendiente()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sesion = await PosFixture.CrearSesionAsync(db, montoApertura: 500m);
        var ctx = await PosVentaFixture.PrepararAsync(db, sesion);

        var provider = new TestEntidadProvider(TestDb.EntidadIdSeed, TestDb.UsuarioIdSeed, isMaster: true);
        var parametros = new ParametroSistemaService(db);
        var contabilidad = new AccountingService(db, null!, provider, new ConsecutivoService(db));
        var inventario = new InventoryService(db, contabilidad, parametros, provider, NullLogger<InventoryService>.Instance);
        var notificaciones = new NotificacionNula();

        var sync = new PosSincronizacionService(db, notificaciones, inventario, contabilidad, parametros,
            NullLogger<PosSincronizacionService>.Instance, provider, new ConsecutivoService(db));

        // Dos ventas distintas (claves idempotentes distintas) con la misma serie y número: la
        // primera del lote factura; la segunda choca contra el índice único y queda en ERROR.
        var buena = PosVentaFixture.Request(sesion, ctx, cantidad: 2m, precioUnitario: 100m, numeroFactura: "900");
        var venenosa = PosVentaFixture.Request(sesion, ctx, cantidad: 1m, precioUnitario: 100m, numeroFactura: "900");

        await sync.RecibirAsync(buena, CancellationToken.None);
        await sync.RecibirAsync(venenosa, CancellationToken.None);

        // El barrido va por fecha de recibo: la buena debe ir primero.
        var filaBuena = await db.PosVentaPendientes.FirstAsync(p => p.IdempotencyKey == buena.IdempotencyKey);
        var filaVenenosa = await db.PosVentaPendientes.FirstAsync(p => p.IdempotencyKey == venenosa.IdempotencyKey);
        filaBuena.FechaRecibidoServidor = DateTimeOffset.UtcNow.AddMinutes(-10);
        filaVenenosa.FechaRecibidoServidor = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();

        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddSingleton<INotificationService>(notificaciones);
        services.AddSingleton(sync);
        using var serviceProvider = services.BuildServiceProvider();

        var worker = new PosSyncBackgroundWorker(serviceProvider, NullLogger<PosSyncBackgroundWorker>.Instance);
        var barrido = typeof(PosSyncBackgroundWorker)
            .GetMethod("ProcessPendingSalesAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
        await (Task)barrido.Invoke(worker, new object[] { CancellationToken.None })!;

        // La venta buena se factura, mueve inventario y contabiliza...
        var almacenada = await db.PosVentaPendientes.AsNoTracking().FirstAsync(p => p.Id == filaBuena.Id);
        Assert.Equal("PROCESADO", almacenada.Estado);
        Assert.NotNull(almacenada.FacturaId);

        var factura = await db.FacturaVenta.AsNoTracking().FirstAsync(f => f.Id == almacenada.FacturaId);
        Assert.Equal("EMITIDA", factura.Estado);
        Assert.Equal("POS", factura.CanalVenta);
        Assert.NotNull(factura.AsientoId);

        // ...y la venenosa queda ERROR con el motivo de negocio, sin factura asociada, sin
        // consumir existencia (su transacción SERIALIZABLE se revirtió) y sin duplicar factura.
        var venenoDb = await db.PosVentaPendientes.AsNoTracking().FirstAsync(p => p.Id == filaVenenosa.Id);
        Assert.Equal("ERROR", venenoDb.Estado);
        Assert.Contains("Ya existe una factura", venenoDb.MensajeError);
        Assert.Equal(1, venenoDb.IntentosProcesamiento);
        Assert.Null(venenoDb.FacturaId);
        Assert.Equal(1, await db.FacturaVenta.AsNoTracking()
            .CountAsync(f => f.Serie == ctx.Serie && f.NumeroFactura == "900"));

        // Nada a medias de la venta venenosa llegó a persistirse: solo queda el movimiento de
        // inventario de la venta buena. (Su decremento crudo de existencia no se revierte aquí
        // porque el test comparte su transacción ambiente y el servicio no la posee; en
        // producción ProcesarAsync abre —y revierte— su propia transacción SERIALIZABLE.)
        Assert.Equal(1, await db.MovimientoInventarios.AsNoTracking()
            .CountAsync(m => m.ReferenciaExternaTipo == "FACTURA_VENTA" && m.ReferenciaExternaId == factura.Id));

        await tx.RollbackAsync();
    }
}
