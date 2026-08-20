using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IBIService
{
    Task<decimal> CalculateLiquidityAsync(Guid entidadId, Guid periodId);
    Task<decimal> CalculateProfitabilityAsync(Guid entidadId, Guid periodId);
    Task<decimal> CalculateInventoryTurnoverAsync(Guid entidadId, Guid periodId);
    Task<Dictionary<string, decimal>> GetDashboardStatsAsync(Guid entidadId, Guid periodId);
    Task<List<decimal>> GetTrendDataAsync(Guid entidadId, string kpiType, int periodsCount);
}

public class BIService : IBIService
{
    private readonly AppDbContext _context;

    public BIService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> CalculateLiquidityAsync(Guid entidadId, Guid periodId)
    {
        var activos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Codigo.StartsWith("1"))
            .SumAsync(d => d.Debe - d.Haber);

        var pasivos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && (d.Cuenta.Codigo.StartsWith("4") || d.Cuenta.Codigo.StartsWith("203")))
            .SumAsync(d => d.Haber - d.Debe);

        if (pasivos == 0) return activos > 0 ? 9.99m : 0;
        return Math.Round(activos / pasivos, 2);
    }

    public async Task<decimal> CalculateProfitabilityAsync(Guid entidadId, Guid periodId)
    {
        var ingresos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "INGRESOS")
            .SumAsync(d => d.Haber - d.Debe);

        var gastos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "GASTOS")
            .SumAsync(d => d.Debe - d.Haber);

        if (ingresos == 0) return 0;
        return Math.Round((ingresos - gastos) / ingresos * 100, 2);
    }

    public async Task<decimal> CalculateInventoryTurnoverAsync(Guid entidadId, Guid periodId)
    {
        var costoVentas = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Codigo.StartsWith("7") && d.Cuenta.Nombre.Contains("Venta"))
            .SumAsync(d => d.Debe - d.Haber);

        var inventario = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Codigo.StartsWith("3"))
            .SumAsync(d => d.Debe - d.Haber);

        if (inventario == 0) return 0;
        return Math.Round(costoVentas / inventario, 2);
    }

    public async Task<Dictionary<string, decimal>> GetDashboardStatsAsync(Guid entidadId, Guid periodId)
    {
        return new Dictionary<string, decimal>
        {
            { "Liquidez", await CalculateLiquidityAsync(entidadId, periodId) },
            { "Rentabilidad", await CalculateProfitabilityAsync(entidadId, periodId) },
            { "Rotacion", await CalculateInventoryTurnoverAsync(entidadId, periodId) }
        };
    }

    public async Task<List<decimal>> GetTrendDataAsync(Guid entidadId, string kpiType, int periodsCount)
    {
        var lastPeriods = await _context.PeriodoContables
            .Where(p => p.EntidadId == entidadId)
            .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes)
            .Take(periodsCount)
            .Reverse()
            .ToListAsync();

        var trend = new List<decimal>();
        foreach (var p in lastPeriods)
        {
            decimal val = kpiType switch
            {
                "LIQUIDEZ" => await CalculateLiquidityAsync(entidadId, p.Id),
                "RENTABILIDAD" => await CalculateProfitabilityAsync(entidadId, p.Id),
                _ => await CalculateInventoryTurnoverAsync(entidadId, p.Id)
            };
            trend.Add(val);
        }
        return trend;
    }
}
