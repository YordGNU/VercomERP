using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface ITaxService
{
    decimal CalculateSalesTax(decimal amount);
    decimal CalculateSocialSecurityContribution(decimal totalSalary);
    decimal CalculateWorkforceTax(decimal totalSalary);
    Task<decimal> CalculateIncomeTaxAsync(Guid entidadId, Guid periodId);
}

public class TaxService : ITaxService
{
   private readonly AppDbContext _context;  

    public TaxService(AppDbContext context)
    {
        _context = context;
    }

    public decimal CalculateSalesTax(decimal amount)
    {
        // Generalmente 10% en Cuba para MiPyMES (ajustable por configuración)
        return amount * 0.10m;
    }

    public decimal CalculateSocialSecurityContribution(decimal totalSalary)
    {
        // 5% trabajador + 12.5% empleador (simplificado)
        return totalSalary * 0.175m;
    }

    public decimal CalculateWorkforceTax(decimal totalSalary)
    {
        // 5% sobre el total devengado
        return totalSalary * 0.05m;
    }

    public async Task<decimal> CalculateIncomeTaxAsync(Guid entidadId, Guid periodId)
    {
        // Utilidad = Ingresos - Gastos
        var ingresos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "INGRESOS" && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => d.Haber - d.Debe);

        var gastos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "GASTOS" && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => d.Debe - d.Haber);

        var utilidad = ingresos - gastos;
        if (utilidad <= 0) return 0;

        // Tasa general 35% MiPyMES (Ley 113)
        return utilidad * 0.35m;
    }
}
