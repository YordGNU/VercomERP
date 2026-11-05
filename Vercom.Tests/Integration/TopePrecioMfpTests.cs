using Microsoft.EntityFrameworkCore;
using Vercom.Tests.Infrastructure;
using VERCOM.Models;
using VERCOM.Services;
using Xunit;

namespace Vercom.Tests.Integration;

/// <summary>
/// RF-55: tope máximo de precio del MFP. La validación vive dentro de
/// <c>SalesService.CreateInvoiceAsync</c>, que abre su propia transacción, así que
/// no se puede envolver en la del test: el tope temporal se elimina en un
/// <c>finally</c>. La comprobación ocurre en la fase de pre-validación, antes de
/// generar movimientos de inventario, de modo que una venta bloqueada no deja
/// rastro alguno.
/// </summary>
public class TopePrecioMfpTests
{
    private static SalesService CrearServicio(AppDbContext db)
        => new(
            db,
            null!,
            new ContractService(db),
            null!,
            null!,
            new ConsecutivoService(db),
            null!,
            new ParametroSistemaService(db),
            new TestEntidadProvider(TestDb.EntidadIdSeed, TestDb.UsuarioIdSeed, isMaster: true),
            null!);

    [DbFact(RequiereDatosBase = true)]
    public async Task VentaPorEncimaDelTopeDeProductoSeBloqueaYNoDejaFactura()
    {
        await using var db = TestDb.Create();
        var ctx = await VentaFixture.ResolverContextoAsync(db);
        Assert.NotNull(ctx);

        var topeId = await VentaFixture.InsertarTopeAsync(db, ctx!.ProductoId, null, precioMaximo: 10m);

        try
        {
            var facturasAntes = await db.FacturaVenta.CountAsync();
            var servicio = CrearServicio(db);

            var (exito, mensaje, factura) = await servicio.CreateInvoiceAsync(
                VentaFixture.CrearFactura(ctx, precioUnitario: 999m));

            Assert.False(exito);
            Assert.Null(factura);
            Assert.Contains("ALERTA LEGAL", mensaje);
            Assert.Contains("tope", mensaje, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(facturasAntes, await db.FacturaVenta.CountAsync());
        }
        finally
        {
            await VentaFixture.EliminarTopeAsync(db, topeId);
        }
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task VentaPorEncimaDelTopeDeLaFamiliaTambienSeBloquea()
    {
        await using var db = TestDb.Create();
        var ctx = await VentaFixture.ResolverContextoAsync(db);
        Assert.NotNull(ctx);
        Assert.NotNull(ctx!.FamiliaId);

        var topeId = await VentaFixture.InsertarTopeAsync(db, null, ctx.FamiliaId, precioMaximo: 10m);

        try
        {
            var servicio = CrearServicio(db);

            var (exito, mensaje, _) = await servicio.CreateInvoiceAsync(
                VentaFixture.CrearFactura(ctx, precioUnitario: 999m));

            Assert.False(exito);
            Assert.Contains("ALERTA LEGAL", mensaje);
        }
        finally
        {
            await VentaFixture.EliminarTopeAsync(db, topeId);
        }
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task UnProductoInexistenteSeRechazaAntesDeEvaluarElTope()
    {
        await using var db = TestDb.Create();
        var ctx = await VentaFixture.ResolverContextoAsync(db);
        Assert.NotNull(ctx);

        var servicio = CrearServicio(db);

        var (exito, mensaje, factura) = await servicio.CreateInvoiceAsync(
            VentaFixture.CrearFactura(ctx!, precioUnitario: 1m, productoId: Guid.NewGuid()));

        Assert.False(exito);
        Assert.Null(factura);
        Assert.Contains("no encontrado", mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ALERTA LEGAL", mensaje);
    }
}
