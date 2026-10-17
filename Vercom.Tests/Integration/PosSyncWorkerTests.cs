using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Vercom.Tests.Infrastructure;
using VERCOM.Models;
using VERCOM.Security;
using VERCOM.Services;
using VERCOM.ViewModels;
using Xunit;

namespace Vercom.Tests.Integration;

/// <summary>
/// Barrido del worker de sincronización POS: una fila irrecuperable no puede bloquear
/// la cola (era el fallo de la rama legacy, que abortaba el barrido completo).
/// </summary>
public class PosSyncWorkerTests
{
    [DbFact(RequiereDatosBase = true)]
    public async Task UnaFilaLegacyQueFallaNoBloqueaElBarrido()
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
            NullLogger<PosSincronizacionService>.Instance);

        // Fila venenosa legacy (sin TipoMovimientoId) que falla al procesarse, más antigua
        // para que el worker la atienda primero.
        var venenosa = new PosVentaPendiente
        {
            Id = Guid.NewGuid(),
            DispositivoPosId = sesion.DispositivoPosId,
            SesionCajaPosId = sesion.SesionId,
            IdempotencyKey = $"poison-{Guid.NewGuid():N}",
            PayloadJson = "{\"LocalId\":1,\"Moneda\":\"CUP\"}",
            FechaVentaLocal = DateTimeOffset.UtcNow.AddMinutes(-10),
            FechaRecibidoServidor = DateTimeOffset.UtcNow.AddMinutes(-10),
            Estado = "PENDIENTE",
            IntentosProcesamiento = 0
        };
        db.PosVentaPendientes.Add(venenosa);
        await db.SaveChangesAsync();

        // Venta v2 válida, más nueva: el mismo barrido debe alcanzarla.
        var request = PosVentaFixture.Request(sesion, ctx, cantidad: 2m, precioUnitario: 100m);
        var recibida = await sync.RecibirAsync(request, CancellationToken.None);
        Assert.Null(recibida.Error);

        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddSingleton<INotificationService>(notificaciones);
        services.AddSingleton<ISalesService>(new SalesServiceFalso(db));
        services.AddSingleton(sync);
        using var serviceProvider = services.BuildServiceProvider();

        var worker = new PosSyncBackgroundWorker(serviceProvider, NullLogger<PosSyncBackgroundWorker>.Instance);
        var barrido = typeof(PosSyncBackgroundWorker)
            .GetMethod("ProcessPendingSalesAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
        await (Task)barrido.Invoke(worker, new object[] { CancellationToken.None })!;

        // La fila venenosa queda ERROR (visible, sin reintento infinito)...
        var venenosaDb = await db.PosVentaPendientes.AsNoTracking().FirstAsync(p => p.Id == venenosa.Id);
        Assert.Equal("ERROR", venenosaDb.Estado);
        Assert.Contains("legacy no soportado", venenosaDb.MensajeError);
        Assert.Equal(1, venenosaDb.IntentosProcesamiento);

        // La factura a medio insertar por el intento legacy no queda persistida.
        Assert.Equal(0, await db.FacturaVenta.AsNoTracking().CountAsync(f => f.CreadoPor == Guid.Empty));

        // ...y la venta v2 se procesa igual en el mismo barrido.
        var v2 = await db.PosVentaPendientes.AsNoTracking().FirstAsync(p => p.IdempotencyKey == request.IdempotencyKey);
        Assert.Equal("PROCESADO", v2.Estado);
        Assert.NotNull(v2.FacturaId);

        var factura = await db.FacturaVenta.AsNoTracking().FirstAsync(f => f.Id == v2.FacturaId);
        Assert.Equal("EMITIDA", factura.Estado);
        Assert.Equal("POS", factura.CanalVenta);
        Assert.NotNull(factura.AsientoId);

        await tx.RollbackAsync();
    }

    // El fallo de la rama legacy se simula aquí: en producción lo disparan la FK
    // creado_por = Guid.Empty, el cliente/almacén vacíos o el estado inválido. La factura
    // se agrega al contexto antes de fallar para reproducir el insert que no persiste.
    private sealed class SalesServiceFalso : ISalesService
    {
        private readonly AppDbContext _db;

        public SalesServiceFalso(AppDbContext db) => _db = db;

        public Task<IEnumerable<FacturaVentum>> GetInvoicesAsync(string? search = null, string? status = null, string? channel = null)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<PrecioVentaDto>> GetSalePricesAsync(Guid? clienteId, DateOnly fecha, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<FacturaVentum?> GetInvoiceByIdAsync(Guid id)
            => throw new NotSupportedException();

        public Task<SalesCreateViewModel> GetSalesCreateContextAsync(FacturaVentum? existingInvoice = null)
            => throw new NotSupportedException();

        public Task<(bool Succeeded, string Message, FacturaVentum? Invoice)> CreateInvoiceAsync(FacturaVentum invoice)
        {
            _db.FacturaVenta.Add(invoice);
            throw new InvalidOperationException("legacy no soportado");
        }

        public Task<(bool Succeeded, string Message)> CancelInvoiceAsync(Guid invoiceId, string reason)
            => throw new NotSupportedException();
    }
}
