using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IPayrollService
{
    Task<(bool Succeeded, string Message)> CalculatePayrollAsync(Guid entidadId, short anio, short mes);
    Task<List<NominaDetalle>> GetPayrollDetailsAsync(Guid periodId);
    Task<(bool Succeeded, string Message)> ApprovePayrollAsync(Guid periodId, Guid userId);
}

public class PayrollService : IPayrollService
{
    private readonly AppDbContext _context;
    private readonly IAccountingService _accountingService;

    public PayrollService(AppDbContext context, IAccountingService accountingService)
    {
        _context = context;
        _accountingService = accountingService;
    }

    public async Task<(bool Succeeded, string Message)> CalculatePayrollAsync(Guid entidadId, short anio, short mes)
    {
        // 1. Obtener o crear periodo de nómina
        var period = await _context.PeriodoNominas
            .FirstOrDefaultAsync(p => p.EntidadId == entidadId && p.Anio == anio && p.Mes == mes);

        if (period == null)
        {
            period = new PeriodoNomina
            {
                Id = Guid.NewGuid(),
                EntidadId = entidadId,
                Anio = anio,
                Mes = mes,
                Tipo = "MENSUAL",
                Estado = "PRENOMINA"
            };
            _context.PeriodoNominas.Add(period);
        }
        else if (period.Estado != "PRENOMINA")
        {
            return (false, "El periodo de nómina ya está calculado o aprobado.");
        }

        // 2. Limpiar cálculos previos
        var existingDetails = _context.NominaDetalles.Where(d => d.PeriodoNominaId == period.Id);
        _context.NominaDetalles.RemoveRange(existingDetails);

        // 3. Obtener empleados activos
        var employees = await _context.Empleados
            .Include(e => e.ContratoLaborals)
            .Include(e => e.Cargo)
            .Where(e => e.EntidadId == entidadId && e.Estado == "ACTIVO")
            .ToListAsync();

        foreach (var emp in employees)
        {
            var contract = emp.ContratoLaborals.FirstOrDefault(c => c.Estado == "VIGENTE");
            if (contract == null) continue;

            // Días trabajados en el mes (Simplificado: 24 días laborales base)
            var attendanceCount = await _context.RegistroAsistencia
                .CountAsync(a => a.EmpleadoId == emp.Id && a.Fecha.Year == anio && a.Fecha.Month == mes && a.TipoAusenciaId == null);

            var scaleSalary = contract.SalarioPactado;
            var dailyRate = scaleSalary / 24;
            var earnedSalary = dailyRate * attendanceCount;

            // Horas extra
            var overtimeHours = await _context.RegistroAsistencia
                .Where(a => a.EmpleadoId == emp.Id && a.Fecha.Year == anio && a.Fecha.Month == mes)
                .SumAsync(a => a.HorasExtra);

            var overtimeAmount = dailyRate / 8 * overtimeHours * 2; // Pago doble por simplificación

            // Retención Seguridad Social (5%)
            var ssRetention = (earnedSalary + overtimeAmount) * 0.05m;

            var detail = new NominaDetalle
            {
                Id = Guid.NewGuid(),
                PeriodoNominaId = period.Id,
                EmpleadoId = emp.Id,
                DiasTrabajados = attendanceCount,
                HorasExtra = overtimeHours,
                SalarioDevengado = earnedSalary + overtimeAmount,
                TotalDeducciones = ssRetention,
                SalarioNeto = (earnedSalary + overtimeAmount) - ssRetention
            };

            _context.NominaDetalles.Add(detail);
        }

        period.CalculadoEn = DateTimeOffset.Now;
        await _context.SaveChangesAsync();

        return (true, "Nómina calculada exitosamente.");
    }

    public async Task<List<NominaDetalle>> GetPayrollDetailsAsync(Guid periodId)
    {
        return await _context.NominaDetalles
            .Include(d => d.Empleado)
            .Where(d => d.PeriodoNominaId == periodId)
            .ToListAsync();
    }

    public async Task<(bool Succeeded, string Message)> ApprovePayrollAsync(Guid periodId, Guid userId)
    {
        var period = await _context.PeriodoNominas
            .Include(p => p.NominaDetalles)
            .FirstOrDefaultAsync(p => p.Id == periodId);

        if (period == null) return (false, "Periodo no encontrado.");
        if (period.Estado != "PRENOMINA") return (false, "La nómina ya fue aprobada.");

        // Generar asiento contable automático
        var totalDevengado = period.NominaDetalles.Sum(d => d.SalarioDevengado);
        var totalRetenciones = period.NominaDetalles.Sum(d => d.TotalDeducciones);
        var totalNeto = period.NominaDetalles.Sum(d => d.SalarioNeto);

        var entry = new AsientoContable
        {
            Id = Guid.NewGuid(),
            EntidadId = period.EntidadId,
            Fecha = DateOnly.FromDateTime(DateTime.Now),
            Concepto = $"CONTABILIZACIÓN NÓMINA {period.Mes}/{period.Anio}",
            ModuloOrigen = "NOMINA",
            DocumentoOrigenTipo = "PERIODO_NOMINA",
            DocumentoOrigenId = period.Id,
            TipoComprobanteId = 8, // Nómina (NO)
            CreadoPor = userId,
            CreadoEn = DateTimeOffset.Now
        };

        // Gasto de Salarios (Debe) - Usar cuenta 701 de SeedData
        var expenseAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == "701");
        if (expenseAccount != null)
        {
            entry.AsientoDetalles.Add(new AsientoDetalle
            {
                Id = Guid.NewGuid(),
                CuentaId = expenseAccount.Id,
                Debe = totalDevengado,
                Glosa = "Gasto de Salarios del Mes"
            });
        }

        // Retenciones por Pagar (Haber) - Usar cuenta 401 de SeedData
        var payableAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == "401");
        if (payableAccount != null)
        {
            entry.AsientoDetalles.Add(new AsientoDetalle
            {
                Id = Guid.NewGuid(),
                CuentaId = payableAccount.Id,
                Haber = totalRetenciones,
                Glosa = "Retenciones Seg. Social (5%)"
            });

            entry.AsientoDetalles.Add(new AsientoDetalle
            {
                Id = Guid.NewGuid(),
                CuentaId = payableAccount.Id,
                Haber = totalNeto,
                Glosa = "Salarios por Pagar"
            });
        }

        var result = await _accountingService.CreateEntryAsync(entry);
        if (!result.Succeeded) return (false, $"Error contable: {result.Message}");

        period.Estado = "APROBADA";
        period.AprobadoPor = userId;
        period.AsientoId = entry.Id;

        await _context.SaveChangesAsync();
        return (true, "Nómina aprobada y contabilizada correctamente.");
    }
}
