using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Vercom.Tests.Infrastructure;
using VERCOM.Models;
using VERCOM.Security;
using VERCOM.Services;
using Xunit;

namespace Vercom.Tests.Integration;

/// <summary>
/// Flujo de venta del POS de extremo a extremo: recibo idempotente, procesamiento
/// serializable, factura + movimiento de inventario + asientos + totales de sesión.
/// Todo corre dentro de la transacción del test (rollback al final).
/// </summary>
public class PosVentaE2ETests
{
    private static PosSincronizacionService CrearServicio(AppDbContext db, Guid? entidadDelToken = null)
    {
        var provider = new TestEntidadProvider(TestDb.EntidadIdSeed, TestDb.UsuarioIdSeed, isMaster: true);
        var parametros = new ParametroSistemaService(db);
        var contabilidad = new AccountingService(db, null!, provider, new ConsecutivoService(db));
        var inventario = new InventoryService(db, contabilidad, parametros, provider, NullLogger<InventoryService>.Instance);

        // Sin entidadDelToken el servicio va con el proveedor MASTER del fixture; con una
        // entidad explícita simula el token de un dispositivo real (no master, con su entidad).
        IEntidadProvider solicitante = entidadDelToken is Guid entidad
            ? new TestEntidadProvider(entidad, TestDb.UsuarioIdSeed)
            : provider;

        return new PosSincronizacionService(db, new NotificacionNula(), inventario, contabilidad, parametros,
            NullLogger<PosSincronizacionService>.Instance, solicitante);
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task UnaVentaPosSeProcesaConFacturaMovimientoAsientoEInventarioYArqueo()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sesion = await PosFixture.CrearSesionAsync(db, montoApertura: 500m);
        var ctx = await PosVentaFixture.PrepararAsync(db, sesion);
        var servicio = CrearServicio(db);

        var request = PosVentaFixture.Request(sesion, ctx, cantidad: 2m, precioUnitario: 100m);
        var recibida = await servicio.RecibirAsync(request, CancellationToken.None);
        Assert.Null(recibida.Error);
        Assert.Equal("PENDIENTE", recibida.Result!.Estado);

        var (result, error, conflict) = await servicio.ProcesarAsync(
            sesion.DispositivoPosId, request.IdempotencyKey, CancellationToken.None);

        Assert.Null(error);
        Assert.False(conflict);
        Assert.NotNull(result);
        Assert.Equal("PROCESADO", result!.Estado);
        Assert.NotNull(result.FacturaId);

        var factura = await db.FacturaVenta.AsNoTracking().FirstAsync(f => f.Id == result.FacturaId);
        Assert.Equal("EMITIDA", factura.Estado);
        Assert.Equal("POS", factura.CanalVenta);
        Assert.Equal("TST", factura.Serie);
        Assert.Equal(200m, factura.Total);
        Assert.Equal(sesion.CajeroId, factura.CreadoPor);

        // La venta tiene que mover inventario y dejar el asiento de venta cuadrado.
        var movimiento = await db.MovimientoInventarios.AsNoTracking()
            .Include(m => m.MovimientoInventarioDetalles)
            .FirstAsync(m => m.ReferenciaExternaId == factura.Id);
        Assert.Equal("POS", movimiento.Canal);
        Assert.Equal(ctx.AlmacenId, movimiento.AlmacenOrigenId);
        Assert.Equal(2m, movimiento.MovimientoInventarioDetalles.Sum(d => d.Cantidad));

        Assert.NotNull(factura.AsientoId);
        var asiento = await db.AsientoContables.AsNoTracking()
            .Include(a => a.AsientoDetalles)
            .FirstAsync(a => a.Id == factura.AsientoId);
        Assert.Equal("CONTABILIZADO", asiento.Estado);
        Assert.Equal("VENTAS", asiento.ModuloOrigen);
        Assert.Equal(factura.Total, asiento.AsientoDetalles.Sum(d => d.Debe));
        Assert.Equal(factura.Total, asiento.AsientoDetalles.Sum(d => d.Haber));

        // DEBE caja por el cobro y HABER ingresos por la venta.
        var ctaCaja = await CuentaAsync(db, ctx.EntidadId, "CTA_CAJA_MN", "101");
        var ctaIngresos = await CuentaAsync(db, ctx.EntidadId, "CTA_VENTAS_GENERAL", "900.0000");
        Assert.Contains(asiento.AsientoDetalles, d => d.CuentaId == ctaCaja && d.Debe == 200m);
        Assert.Contains(asiento.AsientoDetalles, d => d.CuentaId == ctaIngresos && d.Haber == 200m);

        // El almacén del dispositivo queda decrementado exactamente por lo vendido.
        var existencia = await db.Existencia.AsNoTracking()
            .FirstAsync(e => e.AlmacenId == ctx.AlmacenId && e.ProductoId == ctx.ProductoId);
        Assert.Equal(ctx.ExistenciaInicial - 2m, existencia.Cantidad);

        // El movimiento también contabiliza su costo (DEBE costo de venta / HABER inventario).
        Assert.NotNull(movimiento.AsientoId);
        var costoCosto = await CuentaAsync(db, ctx.EntidadId, "CTA_COSTO_VENTA_GENERAL", "810.0000");
        var costoInventario = await CuentaAsync(db, ctx.EntidadId, "CTA_INV_GENERICA", "183.0010");
        var asientoCosto = await db.AsientoContables.AsNoTracking()
            .Include(a => a.AsientoDetalles)
            .FirstAsync(a => a.Id == movimiento.AsientoId);
        Assert.Equal("INVENTARIO", asientoCosto.ModuloOrigen);
        Assert.Equal(asientoCosto.AsientoDetalles.Sum(d => d.Debe), asientoCosto.AsientoDetalles.Sum(d => d.Haber));
        Assert.Contains(asientoCosto.AsientoDetalles, d => d.CuentaId == costoCosto && d.Debe == 120m);
        Assert.Contains(asientoCosto.AsientoDetalles, d => d.CuentaId == costoInventario && d.Haber == 120m);

        // Y la sesión refleja la venta para el arqueo.
        var sesionDb = await db.SesionCajaPos.AsNoTracking().FirstAsync(s => s.Id == sesion.SesionId);
        Assert.Equal(200m, sesionDb.TotalVentas);
        Assert.Equal(200m, sesionDb.TotalEfectivo);
        Assert.Equal(1, sesionDb.CantidadFacturas);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task ReprocesarLaMismaVentaNoDuplicaFacturaNiInventario()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sesion = await PosFixture.CrearSesionAsync(db, montoApertura: 500m);
        var ctx = await PosVentaFixture.PrepararAsync(db, sesion);
        var servicio = CrearServicio(db);

        var request = PosVentaFixture.Request(sesion, ctx, cantidad: 3m, precioUnitario: 50m);
        var recibida = await servicio.RecibirAsync(request, CancellationToken.None);
        Assert.Null(recibida.Error);

        var primera = await servicio.ProcesarAsync(sesion.DispositivoPosId, request.IdempotencyKey, CancellationToken.None);
        Assert.Null(primera.Error);
        Assert.NotNull(primera.Result?.FacturaId);

        var segunda = await servicio.ProcesarAsync(sesion.DispositivoPosId, request.IdempotencyKey, CancellationToken.None);
        Assert.Null(segunda.Error);
        Assert.Equal(primera.Result!.FacturaId, segunda.Result!.FacturaId);

        var facturaId = primera.Result.FacturaId!.Value;
        Assert.Equal(1, await db.FacturaVenta.AsNoTracking().CountAsync(f => f.Id == facturaId));
        Assert.Equal(1, await db.MovimientoInventarios.AsNoTracking().CountAsync(m => m.ReferenciaExternaId == facturaId));
        Assert.Equal(1, await db.AsientoContables.AsNoTracking().CountAsync(a => a.DocumentoOrigenId == facturaId));

        var existencia = await db.Existencia.AsNoTracking()
            .FirstAsync(e => e.AlmacenId == ctx.AlmacenId && e.ProductoId == ctx.ProductoId);
        Assert.Equal(ctx.ExistenciaInicial - 3m, existencia.Cantidad);

        var sesionDb = await db.SesionCajaPos.AsNoTracking().FirstAsync(s => s.Id == sesion.SesionId);
        Assert.Equal(150m, sesionDb.TotalVentas);
        Assert.Equal(1, sesionDb.CantidadFacturas);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task RecibirLaMismaVentaDosVecesNoEncolaDuplicados()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sesion = await PosFixture.CrearSesionAsync(db);
        var ctx = await PosVentaFixture.PrepararAsync(db, sesion);
        var servicio = CrearServicio(db);

        var clave = $"test-{Guid.NewGuid():N}";
        var primera = await servicio.RecibirAsync(PosVentaFixture.Request(sesion, ctx, idempotencyKey: clave), CancellationToken.None);
        var segunda = await servicio.RecibirAsync(PosVentaFixture.Request(sesion, ctx, idempotencyKey: clave), CancellationToken.None);

        Assert.Null(primera.Error);
        Assert.Null(segunda.Error);
        Assert.Equal(primera.Result!.Id, segunda.Result!.Id);
        Assert.Equal(1, await db.PosVentaPendientes.AsNoTracking().CountAsync(p => p.IdempotencyKey == clave));

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task LosPagosQueNoCuadranConElTotalSeRechazanSinFacturar()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sesion = await PosFixture.CrearSesionAsync(db);
        var ctx = await PosVentaFixture.PrepararAsync(db, sesion);
        var servicio = CrearServicio(db);

        // Total real 200 (2 x 100) pero se declara un pago de 150.
        var request = PosVentaFixture.Request(sesion, ctx, cantidad: 2m, precioUnitario: 100m, montoPago: 150m);
        await servicio.RecibirAsync(request, CancellationToken.None);

        var (result, error, conflict) = await servicio.ProcesarAsync(
            sesion.DispositivoPosId, request.IdempotencyKey, CancellationToken.None);

        Assert.Null(result);
        Assert.NotNull(error);
        Assert.False(conflict);
        Assert.Contains("pagos", error, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, await db.FacturaVenta.AsNoTracking().CountAsync(f => f.Serie == ctx.Serie));

        var pendiente = await db.PosVentaPendientes.AsNoTracking().FirstAsync(p => p.IdempotencyKey == request.IdempotencyKey);
        Assert.Equal("PENDIENTE", pendiente.Estado);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task SiElAsientoDeCostoNoSePuedeResolverLaVentaIgualSeProcesa()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sesion = await PosFixture.CrearSesionAsync(db, montoApertura: 500m);
        var ctx = await PosVentaFixture.PrepararAsync(db, sesion);
        await PosVentaFixture.ForzarFalloDelAsientoDeCostoAsync(db, ctx);
        var servicio = CrearServicio(db);

        var request = PosVentaFixture.Request(sesion, ctx, cantidad: 2m, precioUnitario: 100m);
        await servicio.RecibirAsync(request, CancellationToken.None);

        var (result, error, conflict) = await servicio.ProcesarAsync(
            sesion.DispositivoPosId, request.IdempotencyKey, CancellationToken.None);

        // Un hueco de datos contables no debe tumbar la venta ni bloquear la cola.
        Assert.Null(error);
        Assert.False(conflict);
        Assert.Equal("PROCESADO", result!.Estado);

        var factura = await db.FacturaVenta.AsNoTracking().FirstAsync(f => f.Id == result.FacturaId);
        Assert.Equal("EMITIDA", factura.Estado);
        Assert.NotNull(factura.AsientoId);

        var movimiento = await db.MovimientoInventarios.AsNoTracking().FirstAsync(m => m.ReferenciaExternaId == factura.Id);
        Assert.Null(movimiento.AsientoId);
        Assert.Equal(0, await db.AsientoContables.AsNoTracking()
            .CountAsync(a => a.ModuloOrigen == "INVENTARIO" && a.DocumentoOrigenId == movimiento.Id));

        var existencia = await db.Existencia.AsNoTracking()
            .FirstAsync(e => e.AlmacenId == ctx.AlmacenId && e.ProductoId == ctx.ProductoId);
        Assert.Equal(ctx.ExistenciaInicial - 2m, existencia.Cantidad);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task UnTokenDeOtraEntidadNoRegistraNiProcesaNiLeeVentasDelDispositivo()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sesion = await PosFixture.CrearSesionAsync(db);
        var ctx = await PosVentaFixture.PrepararAsync(db, sesion);

        // Token de OTRA entidad: no acepta el recibo del dispositivo.
        var ajeno = CrearServicio(db, Guid.NewGuid());
        var request = PosVentaFixture.Request(sesion, ctx);
        var recibida = await ajeno.RecibirAsync(request, CancellationToken.None);
        Assert.NotNull(recibida.Error);
        Assert.Contains("no encontrado", recibida.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, await db.PosVentaPendientes.AsNoTracking()
            .CountAsync(p => p.IdempotencyKey == request.IdempotencyKey));

        // La dueña sí lo registra...
        var duena = CrearServicio(db, ctx.EntidadId);
        var propia = await duena.RecibirAsync(request, CancellationToken.None);
        Assert.Null(propia.Error);

        // ...pero el token ajeno no puede leer su estado ni procesarla.
        Assert.Null(await ajeno.EstadoAsync(sesion.DispositivoPosId, request.IdempotencyKey, CancellationToken.None));

        var (result, error, _) = await ajeno.ProcesarAsync(sesion.DispositivoPosId, request.IdempotencyKey, CancellationToken.None);
        Assert.Null(result);
        Assert.NotNull(error);
        Assert.Contains("no corresponde al dispositivo", error, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, await db.FacturaVenta.AsNoTracking().CountAsync(f => f.Serie == ctx.Serie));

        // La dueña sí puede leer el estado (misma consulta con filtro por entidad).
        var estado = await duena.EstadoAsync(sesion.DispositivoPosId, request.IdempotencyKey, CancellationToken.None);
        Assert.NotNull(estado);
        Assert.Equal("PENDIENTE", estado!.Estado);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task LaNumeracionFueraDelRangoReservadoSeRechaza()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sesion = await PosFixture.CrearSesionAsync(db);
        var ctx = await PosVentaFixture.PrepararAsync(db, sesion);
        await ReservarRangoAsync(db, sesion, ctx.Serie, desde: 100, hasta: 200);
        var servicio = CrearServicio(db);

        // Dentro del rango reservado al dispositivo: procesa normalmente.
        var dentro = PosVentaFixture.Request(sesion, ctx, numeroFactura: "150");
        await servicio.RecibirAsync(dentro, CancellationToken.None);
        var ok = await servicio.ProcesarAsync(sesion.DispositivoPosId, dentro.IdempotencyKey, CancellationToken.None);
        Assert.Null(ok.Error);
        Assert.Equal("PROCESADO", ok.Result!.Estado);

        // Fuera del rango: se rechaza sin facturar y el pendiente queda visible.
        var fuera = PosVentaFixture.Request(sesion, ctx, numeroFactura: "999");
        await servicio.RecibirAsync(fuera, CancellationToken.None);
        var (result, error, conflict) = await servicio.ProcesarAsync(
            sesion.DispositivoPosId, fuera.IdempotencyKey, CancellationToken.None);

        Assert.Null(result);
        Assert.NotNull(error);
        Assert.True(conflict);
        Assert.Contains("fuera del rango", error);
        Assert.Equal(0, await db.FacturaVenta.AsNoTracking()
            .CountAsync(f => f.Serie == ctx.Serie && f.NumeroFactura == "999"));

        var pendiente = await db.PosVentaPendientes.AsNoTracking()
            .FirstAsync(p => p.IdempotencyKey == fuera.IdempotencyKey);
        Assert.Equal("PENDIENTE", pendiente.Estado);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task LaNumeracionYaUsadaSeReportaComoErrorSinDuplicarFactura()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sesion = await PosFixture.CrearSesionAsync(db);
        var ctx = await PosVentaFixture.PrepararAsync(db, sesion);
        var servicio = CrearServicio(db);

        // Dos ventas distintas (claves idempotentes distintas) con la misma serie y número.
        var primera = PosVentaFixture.Request(sesion, ctx, numeroFactura: "700");
        await servicio.RecibirAsync(primera, CancellationToken.None);
        var ok = await servicio.ProcesarAsync(sesion.DispositivoPosId, primera.IdempotencyKey, CancellationToken.None);
        Assert.Null(ok.Error);
        Assert.Equal("PROCESADO", ok.Result!.Estado);

        var segunda = PosVentaFixture.Request(sesion, ctx, numeroFactura: "700");
        await servicio.RecibirAsync(segunda, CancellationToken.None);
        var (result, error, conflict) = await servicio.ProcesarAsync(
            sesion.DispositivoPosId, segunda.IdempotencyKey, CancellationToken.None);

        // El índice único (entidad, sucursal, serie, número) se reporta como error de negocio,
        // no como fallo 500, y la venta duplicada no se persiste.
        Assert.Null(result);
        Assert.NotNull(error);
        Assert.False(conflict);
        Assert.Contains("Ya existe una factura", error);
        Assert.Equal(1, await db.FacturaVenta.AsNoTracking()
            .CountAsync(f => f.Serie == ctx.Serie && f.NumeroFactura == "700"));

        var pendienteDuplicado = await db.PosVentaPendientes.AsNoTracking()
            .FirstAsync(p => p.IdempotencyKey == segunda.IdempotencyKey);
        Assert.Equal("PENDIENTE", pendienteDuplicado.Estado);

        // El fallo no puede dejar el pendiente marcado PROCESADO con una factura inexistente:
        // cualquier SaveChanges posterior (el del worker) lo persistiría.
        var rastreado = db.ChangeTracker.Entries<PosVentaPendiente>()
            .First(e => e.Entity.IdempotencyKey == segunda.IdempotencyKey);
        Assert.Equal("PENDIENTE", rastreado.Entity.Estado);
        Assert.Null(rastreado.Entity.FacturaId);

        // Y el llamador puede volver a fallar limpiamente: nada quedó a medio insertar.
        var repetido = await servicio.ProcesarAsync(sesion.DispositivoPosId, segunda.IdempotencyKey, CancellationToken.None);
        Assert.NotNull(repetido.Error);
        Assert.Equal(1, await db.FacturaVenta.AsNoTracking()
            .CountAsync(f => f.Serie == ctx.Serie && f.NumeroFactura == "700"));

        await tx.RollbackAsync();
    }

    private static async Task ReservarRangoAsync(AppDbContext db, PosFixture.SesionPos sesion, string serie, long desde, long hasta)
    {
        db.PosRangoNumeracions.Add(new PosRangoNumeracion
        {
            Id = Guid.NewGuid(),
            DispositivoPosId = sesion.DispositivoPosId,
            TipoDocumento = "FACTURA_VENTA",
            Serie = serie,
            NumeroDesde = desde,
            NumeroHasta = hasta,
            NumeroSiguienteLocal = desde,
            AsignadoEn = DateTimeOffset.UtcNow,
            Agotado = false
        });
        await db.SaveChangesAsync();
    }

    private static async Task<Guid> CuentaAsync(AppDbContext db, Guid entidadId, string codigoParametro, string codigoDefecto)
    {
        var parametros = new ParametroSistemaService(db);
        var configurado = await parametros.ObtenerValorVigenteAsync(entidadId, codigoParametro);

        foreach (var codigo in new[] { configurado, codigoDefecto })
        {
            if (string.IsNullOrWhiteSpace(codigo)) continue;

            var id = await db.CuentaContables.AsNoTracking()
                .Where(c => c.Codigo == codigo && c.EntidadId == entidadId)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync();

            if (id is not null) return id.Value;
        }

        throw new InvalidOperationException($"No se encontró la cuenta {codigoParametro}/{codigoDefecto}.");
    }
}
