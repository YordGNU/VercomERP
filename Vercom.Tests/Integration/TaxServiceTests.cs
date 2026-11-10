using Vercom.Tests.Infrastructure;
using VERCOM.Services;
using Xunit;

namespace Vercom.Tests.Integration;

/// <summary>
/// Impuestos parametrizados por entidad. Regresión directa de dos fallos
/// históricos documentados en AGENTS.md:
///  · las ventas NO cobraban impuesto porque faltaba el parámetro TAX_VENTA;
///  · el cálculo salía 0 si el parámetro no existía.
/// </summary>
public class TaxServiceTests
{
    private const string CodigoInexistente = "CODIGO_QUE_NO_EXISTE_XYZ";

    [DbFact]
    public async Task ElParametroDeImpuestoDeVentaExisteYEsPositivo()
    {
        await using var db = TestDb.Create();
        var parametros = new ParametroSistemaService(db);

        var tasa = await parametros.ObtenerValorNumericoVigenteAsync(TestDb.EntidadIdSeed, "TAX_VENTA");

        Assert.True(
            tasa > 0m,
            "Falta el parámetro TAX_VENTA para la entidad: las ventas se registrarían sin impuesto sobre ventas.");
    }

    [DbFact]
    public async Task CalculateSalesTaxAplicaLaTasaConfigurada()
    {
        await using var db = TestDb.Create();
        var parametros = new ParametroSistemaService(db);
        var impuesto = new TaxService(db, null!, parametros);

        var tasa = await parametros.ObtenerValorNumericoVigenteAsync(TestDb.EntidadIdSeed, "TAX_VENTA");
        var calculado = await impuesto.CalculateSalesTaxAsync(TestDb.EntidadIdSeed, 100m);

        Assert.Equal(100m * tasa, calculado);
    }

    [DbFact]
    public async Task LasContribucionesDeNominaNuncaDevuelvenCeroConBasePositiva()
    {
        await using var db = TestDb.Create();
        var parametros = new ParametroSistemaService(db);
        var impuesto = new TaxService(db, null!, parametros);

        // Seguridad social y fuerza de trabajo tienen fallback del 5%: con una
        // base salarial positiva el resultado debe ser > 0 aunque el parámetro falte.
        Assert.True(await impuesto.CalculateSocialSecurityContributionAsync(TestDb.EntidadIdSeed, 1000m) > 0m);
        Assert.True(await impuesto.CalculateWorkforceTaxAsync(TestDb.EntidadIdSeed, 1000m) > 0m);
    }

    [DbFact]
    public async Task UnParametroInexistenteDevuelveCero()
    {
        await using var db = TestDb.Create();
        var parametros = new ParametroSistemaService(db);

        Assert.Equal(0m, await parametros.ObtenerValorNumericoVigenteAsync(TestDb.EntidadIdSeed, CodigoInexistente));
    }
}
