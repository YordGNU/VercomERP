using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IBIService
{
    Task<decimal> CalculateLiquidityAsync(Guid entidadId, Guid periodId);
    Task<decimal> CalculateProfitabilityAsync(Guid entidadId, Guid periodId);
    Task<decimal> CalculateInventoryTurnoverAsync(Guid entidadId, Guid periodId);
    Task<Dictionary<string, decimal>> GetDashboardStatsAsync(Guid entidadId, Guid periodId);
}

public class BIService : IBIService
{
   private readonly AppDbContext _context;  
    private readonly IAccountingService _accountingService;

    public BIService(AppDbContext context, IAccountingService accountingService)
    {
        _context = context;
        _accountingService = accountingService;
    }

    public async Task<decimal> CalculateLiquidityAsync(Guid entidadId, Guid periodId)
    {
        // Liquidez = Activo Corriente / Pasivo Corriente
        // Simplificado: Cuentas 1xx / Cuentas 4xx
        var activos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Codigo.StartsWith("1"))
            .SumAsync(d => d.Debe - d.Haber);

        var pasivos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Codigo.StartsWith("4"))
            .SumAsync(d => d.Haber - d.Debe);

        if (pasivos == 0) return activos > 0 ? 999 : 0;
        return Math.Round(activos / pasivos, 2);
    }

    public async Task<decimal> CalculateProfitabilityAsync(Guid entidadId, Guid periodId)
    {
        // Rentabilidad = Utilidad Neta / Ingresos
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
        // Rotación = Costo de Ventas / Inventario Promedio
        var costoVentas = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Codigo == "701") // Ajustar según nomenclador
            .SumAsync(d => d.Debe - d.Haber);

        var inventario = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Codigo == "301")
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
}
