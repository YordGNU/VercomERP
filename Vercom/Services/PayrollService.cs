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
    private readonly IParametroSistemaService _paramService;

    public PayrollService(AppDbContext context, IAccountingService accountingService, IParametroSistemaService paramService)
    {
        _context = context;
        _accountingService = accountingService;
        _paramService = paramService;
    }

    public async Task<(bool Succeeded, string Message)> CalculatePayrollAsync(Guid entidadId, short anio, short mes)
    {
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

        var existingDetails = _context.NominaDetalles.Where(d => d.PeriodoNominaId == period.Id);
        _context.NominaDetalles.RemoveRange(existingDetails);

        var employees = await _context.Empleados
            .Include(e => e.ContratoLaborals)
            .Include(e => e.Cargo)
            .Where(e => e.EntidadId == entidadId && e.Estado == "ACTIVO")
            .ToListAsync();

        var ssTasa = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "RET_SS_TRAB");
        if (ssTasa == 0) ssTasa = 0.05m;

        foreach (var emp in employees)
        {
            var contract = emp.ContratoLaborals.FirstOrDefault(c => c.Estado == "VIGENTE");
            if (contract == null) continue;

            var attendanceCount = await _context.RegistroAsistencia
                .CountAsync(a => a.EmpleadoId == emp.Id && a.Fecha.Year == anio && a.Fecha.Month == mes && a.TipoAusenciaId == null);

            var scaleSalary = contract.SalarioPactado;
            var dailyRate = scaleSalary / 24;
            var earnedSalary = dailyRate * attendanceCount;

            var overtimeHours = await _context.RegistroAsistencia
                .Where(a => a.EmpleadoId == emp.Id && a.Fecha.Year == anio && a.Fecha.Month == mes)
                .SumAsync(a => a.HorasExtra);

            var overtimeAmount = dailyRate / 8 * overtimeHours * 2;

            var ssRetention = (earnedSalary + overtimeAmount) * ssTasa;

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
            TipoComprobanteId = 8,
            CreadoPor = userId,
            CreadoEn = DateTimeOffset.Now
        };

        var expenseAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == "701" && c.EntidadId == period.EntidadId);
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

        var payableAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == "401" && c.EntidadId == period.EntidadId);
        if (payableAccount != null)
        {
            entry.AsientoDetalles.Add(new AsientoDetalle
            {
                Id = Guid.NewGuid(),
                CuentaId = payableAccount.Id,
                Haber = totalRetenciones,
                Glosa = "Retenciones Seg. Social"
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
