using Microsoft.EntityFrameworkCore;
using Vercom.Tests.Infrastructure;
using Xunit;

namespace Vercom.Tests.Integration;

/// <summary>
/// Consistencia de estados de la cartera (CxC/CxP) contra los CHECK constraints
/// de la BD. Los estados escritos por el código deben caer dentro del conjunto
/// admitido; un estado nuevo no contemplado aquí fallaría en runtime con error
/// 547, por lo que la prueba lo detecta antes.
/// </summary>
public class CarteraEstadoTests
{
    private static readonly string[] EstadosCxcValidos =
        ["PENDIENTE", "PARCIAL", "PAGADA", "INCOBRABLE"];

    private static readonly string[] EstadosCxpValidos =
        ["PENDIENTE", "PARCIAL", "PAGADA"];

    [DbFact]
    public async Task LosEstadosDeCuentasPorCobrarSonValidos()
    {
        await using var db = TestDb.Create();

        var estados = await db.CuentaPorCobrars
            .Select(c => c.Estado)
            .Distinct()
            .ToListAsync();

        var invalidos = estados
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Where(e => !EstadosCxcValidos.Contains(e, StringComparer.OrdinalIgnoreCase))
            .ToList();

        Assert.True(invalidos.Count == 0,
            "Estados de cuenta_por_cobrar fuera del CHECK " +
            $"[{string.Join("/", EstadosCxcValidos)}]: {string.Join(", ", invalidos)}");
    }

    [DbFact]
    public async Task LosEstadosDeCuentasPorPagarSonValidos()
    {
        await using var db = TestDb.Create();

        var estados = await db.CuentaPorPagars
            .Select(c => c.Estado)
            .Distinct()
            .ToListAsync();

        var invalidos = estados
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Where(e => !EstadosCxpValidos.Contains(e, StringComparer.OrdinalIgnoreCase))
            .ToList();

        Assert.True(invalidos.Count == 0,
            "Estados de cuenta_por_pagar fuera del CHECK " +
            $"[{string.Join("/", EstadosCxpValidos)}]: {string.Join(", ", invalidos)}");
    }

    [DbFact]
    public async Task NingunaCuentaPorCobrarTieneSaldoPendienteNegativo()
    {
        await using var db = TestDb.Create();

        var negativas = await db.CuentaPorCobrars
            .Where(c => c.SaldoPendiente < 0)
            .Select(c => new { c.Id, c.SaldoPendiente })
            .ToListAsync();

        Assert.True(negativas.Count == 0,
            "Cuentas por cobrar con saldo pendiente negativo:\n  - " +
            string.Join("\n  - ", negativas.Select(c => $"{c.Id}: {c.SaldoPendiente}")));
    }

    [DbFact]
    public async Task NingunaCuentaPorPagarTieneSaldoPendienteNegativo()
    {
        await using var db = TestDb.Create();

        var negativas = await db.CuentaPorPagars
            .Where(c => c.SaldoPendiente < 0)
            .Select(c => new { c.Id, c.SaldoPendiente })
            .ToListAsync();

        Assert.True(negativas.Count == 0,
            "Cuentas por pagar con saldo pendiente negativo:\n  - " +
            string.Join("\n  - ", negativas.Select(c => $"{c.Id}: {c.SaldoPendiente}")));
    }
}
