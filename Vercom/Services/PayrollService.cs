using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface IPayrollService
{
    // Lectura
    Task<PayrollIndexViewModel> GetPayrollIndexContextAsync();
    Task<PayrollDetailsViewModel?> GetPayrollDetailsContextAsync(Guid periodId);

    // Conceptos de Nómina
    Task<IEnumerable<ConceptoNomina>> GetConceptsAsync();
    Task<(bool Succeeded, string Message)> CreateConceptAsync(ConceptoNomina concept);
    Task<(bool Succeeded, string Message)> UpdateConceptAsync(ConceptoNomina concept);

    // Escritura
    Task<(bool Succeeded, string Message)> CalculatePayrollAsync(Guid entidadId, short anio, short mes);
    Task<List<NominaDetalle>> GetPayrollDetailsAsync(Guid periodId);
    Task<(bool Succeeded, string Message)> ApprovePayrollAsync(Guid periodId, Guid userId);
}

public class PayrollService : IPayrollService
{
    private readonly AppDbContext _context;
    private readonly IAccountingService _accountingService;
    private readonly IParametroSistemaService _paramService;
    private readonly Security.IEntidadProvider _entidadProvider;

    public PayrollService(AppDbContext context, IAccountingService accountingService, IParametroSistemaService paramService, Security.IEntidadProvider entidadProvider)
    {
        _context = context;
        _accountingService = accountingService;
        _paramService = paramService;
        _entidadProvider = entidadProvider;
    }

    public async Task<PayrollIndexViewModel> GetPayrollIndexContextAsync()
    {
        var entidadId = _entidadProvider.CurrentEntidadId;
        return new PayrollIndexViewModel
        {
            Periodos = await _context.PeriodoNominas
                .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes)
                .ToListAsync()
        };
    }

    public async Task<PayrollDetailsViewModel?> GetPayrollDetailsContextAsync(Guid periodId)
    {
        var period = await _context.PeriodoNominas
            .Include(p => p.Asiento)
            .FirstOrDefaultAsync(p => p.Id == periodId);

        if (period == null) return null;

        return new PayrollDetailsViewModel
        {
            Periodo = period,
            Detalles = await _context.NominaDetalles
                .Include(d => d.Empleado)
                .Where(d => d.PeriodoNominaId == periodId)
                .ToListAsync()
        };
    }

    public async Task<IEnumerable<ConceptoNomina>> GetConceptsAsync()
    {
        return await _context.ConceptoNominas.OrderBy(c => c.Nombre).ToListAsync();
    }

    public async Task<(bool Succeeded, string Message)> CreateConceptAsync(ConceptoNomina concept)
    {
        try
        {
            concept.Id = 0;
            concept.EntidadId = _entidadProvider.CurrentEntidadId;
            _context.ConceptoNominas.Add(concept);
            await _context.SaveChangesAsync();
            return (true, "Concepto de nómina creado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateConceptAsync(ConceptoNomina concept)
    {
        try
        {
            var existing = await _context.ConceptoNominas.FindAsync(concept.Id);
            if (existing == null) return (false, "No existe.");
            _context.Entry(existing).CurrentValues.SetValues(concept);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;
            await _context.SaveChangesAsync();
            return (true, "Concepto actualizado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
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

            // RF-22: Pagos por Resultados (Variable)
            var variableAmount = await _context.NominaDetalleConceptos
                .Where(c => c.NominaDetalle.EmpleadoId == emp.Id && c.NominaDetalle.PeriodoNomina.Anio == anio && c.NominaDetalle.PeriodoNomina.Mes == mes)
                .SumAsync(c => (decimal?)c.Monto) ?? 0;

            var brutoTotal = earnedSalary + overtimeAmount + variableAmount;

            // Retenciones Trabajador (RF-23)
            var ssRetention = brutoTotal * ssTasa;

            // Impuesto sobre Ingresos Personales (IRP - Escalado Simplificado para Mipyme)
            decimal irpAmount = 0;
            if (brutoTotal > 3260) {
                irpAmount = (brutoTotal - 3260) * 0.03m; // Ejemplo base: 3% sobre exceso de 3260
            }

            var detail = new NominaDetalle
            {
                Id = Guid.NewGuid(),
                PeriodoNominaId = period.Id,
                EmpleadoId = emp.Id,
                DiasTrabajados = attendanceCount,
                HorasExtra = overtimeHours,
                SalarioDevengado = brutoTotal,
                TotalDeducciones = ssRetention + irpAmount,
                SalarioNeto = brutoTotal - (ssRetention + irpAmount)
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

        // RF-23: Aportes Patronales (Estimados)
        var aporteSS = totalDevengado * 0.125m; // 12.5% Seguridad Social
        var impuestoFT = totalDevengado * 0.05m; // 5% Fuerza de Trabajo

        var entry = new AsientoContable
        {
            Id = Guid.NewGuid(),
            EntidadId = period.EntidadId,
            Fecha = DateOnly.FromDateTime(DateTime.Now),
            Concepto = $"CONTABILIZACIÓN NÓMINA {period.Mes}/{period.Anio} (INCLUYE APORTES PATRONALES)",
            ModuloOrigen = "NOMINA",
            DocumentoOrigenTipo = "PERIODO_NOMINA",
            DocumentoOrigenId = period.Id,
            TipoComprobanteId = 8,
            CreadoPor = userId,
            CreadoEn = DateTimeOffset.Now
        };

        // 1. Gastos de Salarios y Aportes
        var expenseAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == "701" && c.EntidadId == period.EntidadId);
        if (expenseAccount != null)
        {
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = expenseAccount.Id, Debe = totalDevengado, Glosa = "Gasto Salarios Brutos" });
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = expenseAccount.Id, Debe = aporteSS, Glosa = "Gasto Aporte SS (12.5%)" });
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = expenseAccount.Id, Debe = impuestoFT, Glosa = "Gasto Impuesto FT (5%)" });
        }

        // 2. Pasivos y Retenciones
        var payableAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == "401" && c.EntidadId == period.EntidadId);
        if (payableAccount != null)
        {
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = payableAccount.Id, Haber = totalRetenciones, Glosa = "Retenciones Trabajadores (SS+IRP)" });
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = payableAccount.Id, Haber = aporteSS, Glosa = "Seguridad Social por Pagar (Entidad)" });
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = payableAccount.Id, Haber = impuestoFT, Glosa = "Impuesto FT por Pagar" });
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = payableAccount.Id, Haber = totalNeto, Glosa = "Salarios Netos por Pagar" });
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
