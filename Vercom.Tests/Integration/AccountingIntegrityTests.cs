using Microsoft.EntityFrameworkCore;
using Vercom.Tests.Infrastructure;
using Xunit;

namespace Vercom.Tests.Integration;

/// <summary>
/// Invariantes de integridad sobre la BD real (solo lectura).
/// Reproduce el chequeo de la auditoría P1 ("0 asientos descuadrados") como
/// prueba automática permanente, de modo que cualquier cambio en generación de
/// asientos que rompa la partida doble falle aquí en lugar de en producción.
/// </summary>
public class AccountingIntegrityTests
{
    private const decimal Tolerancia = 0.009m;

    [DbFact]
    public async Task NingunAsientoContabilizadoEstaDescuadrado()
    {
        await using var db = TestDb.Create();

        var descuadrados = await db.AsientoContables
            .Where(a => a.Estado == "CONTABILIZADO")
            .Select(a => new
            {
                a.Id,
                a.NumeroComprobante,
                Debe = a.AsientoDetalles.Sum(d => (decimal?)d.Debe) ?? 0m,
                Haber = a.AsientoDetalles.Sum(d => (decimal?)d.Haber) ?? 0m
            })
            .Where(x => x.Debe - x.Haber > Tolerancia || x.Haber - x.Debe > Tolerancia)
            .ToListAsync();

        Assert.True(
            descuadrados.Count == 0,
            "Asientos CONTABILIZADOS descuadrados (Debe != Haber):\n  - " +
            string.Join("\n  - ", descuadrados.Select(x =>
                $"asiento {x.Id} (#{x.NumeroComprobante}): Debe={x.Debe} Haber={x.Haber}")));
    }

    [DbFact]
    public async Task TodoAsientoContabilizadoRespetaLaPartidaDoble()
    {
        await using var db = TestDb.Create();

        var conMenosDeDosLineas = await db.AsientoContables
            .Where(a => a.Estado == "CONTABILIZADO")
            .Where(a => a.AsientoDetalles.Count < 2)
            .Select(a => new { a.Id, Lineas = a.AsientoDetalles.Count })
            .ToListAsync();

        Assert.True(
            conMenosDeDosLineas.Count == 0,
            "Asientos CONTABILIZADOS con menos de 2 líneas (no son partida doble):\n  - " +
            string.Join("\n  - ", conMenosDeDosLineas.Select(x => $"asiento {x.Id}: {x.Lineas} línea(s)")));
    }

    [DbFact]
    public async Task TodoAsientoContabilizadoTieneComprobanteYPeriodo()
    {
        await using var db = TestDb.Create();

        var invalidos = await db.AsientoContables
            .Where(a => a.Estado == "CONTABILIZADO")
            .Where(a => a.NumeroComprobante == 0 || a.PeriodoId == Guid.Empty)
            .Select(a => a.Id)
            .ToListAsync();

        Assert.True(
            invalidos.Count == 0,
            "Asientos CONTABILIZADOS sin número de comprobante o sin periodo:\n  - " +
            string.Join("\n  - ", invalidos));
    }

    [DbFact]
    public async Task TodoPeriodoContableUsaUnEstadoValido()
    {
        var estadosValidos = new[] { "ABIERTO", "CERRADO" };

        await using var db = TestDb.Create();

        var estados = await db.PeriodoContables
            .Select(p => p.Estado)
            .Distinct()
            .ToListAsync();

        var desconocidos = estados
            .Where(e => !estadosValidos.Contains(e, StringComparer.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            desconocidos.Count == 0,
            "Estados de periodo contable fuera del CHECK de BD (ABIERTO/CERRADO): " +
            string.Join(", ", desconocidos));
    }
}
