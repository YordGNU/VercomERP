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
            if (brutoTotal > 3260)
            {
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

        if (period == null) return (false, "Periodo de nómina no encontrado.");
        if (period.Estado != "PRENOMINA" && period.Estado != "CALCULADA")
            return (false, $"La nómina no puede ser aprobada en su estado actual: {period.Estado}");

        if (!period.NominaDetalles.Any())
            return (false, "La nómina no contiene detalles calculados.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var totalDevengado = period.NominaDetalles.Sum(d => d.SalarioDevengado);
            var totalRetenciones = period.NominaDetalles.Sum(d => d.TotalDeducciones);
            var totalNeto = period.NominaDetalles.Sum(d => d.SalarioNeto);

            // 1. Obtener Tasas Dinámicas (RF-23)
            var tasaSSPatronal = await _paramService.ObtenerValorNumericoVigenteAsync(period.EntidadId, "TASA_SS_PATRONAL");
            if (tasaSSPatronal == 0) tasaSSPatronal = 0.125m;

            var tasaFT = await _paramService.ObtenerValorNumericoVigenteAsync(period.EntidadId, "TASA_FUERZA_TRAB");
            if (tasaFT == 0) tasaFT = 0.05m;

            var aporteSS = Math.Round(totalDevengado * tasaSSPatronal, 2);
            var impuestoFT = Math.Round(totalDevengado * tasaFT, 2);

            // 2. Validar Cuentas Contables Obligatorias
            var expenseAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == "701" && c.EntidadId == period.EntidadId);
            var payableAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == "401" && c.EntidadId == period.EntidadId);

            if (expenseAccount == null || !expenseAccount.Activo)
                return (false, "Error de Integración: No se encontró la cuenta de Gasto de Nómina (701) activa.");

            if (payableAccount == null || !payableAccount.Activo)
                return (false, "Error de Integración: No se encontró la cuenta de Pasivo de Nómina (401) activa.");

            // 3. Obtener Tipo de Comprobante (DIA - Diario)
            var tipoComprobante = await _context.TipoComprobantes.FirstOrDefaultAsync(t => t.Codigo == "DIA");
            if (tipoComprobante == null)
                return (false, "Error de Configuración: No se encontró el tipo de comprobante 'DIA' (Diario).");

            var entry = new AsientoContable
            {
                Id = Guid.NewGuid(),
                EntidadId = period.EntidadId,
                PeriodoId = await _context.PeriodoContables
                    .Where(pc => pc.EntidadId == period.EntidadId && pc.Anio == period.Anio && pc.Mes == period.Mes)
                    .Select(pc => pc.Id)
                    .FirstOrDefaultAsync(),
                Fecha = DateOnly.FromDateTime(DateTime.Now),
                Concepto = $"CONTABILIZACIÓN NÓMINA {period.Mes}/{period.Anio} - {period.NominaDetalles.Count} TRABAJADORES",
                ModuloOrigen = "NOMINA",
                DocumentoOrigenTipo = "PERIODO_NOMINA",
                DocumentoOrigenId = period.Id,
                TipoComprobanteId = tipoComprobante.Id,
                CreadoPor = userId,
                CreadoEn = DateTimeOffset.Now,
                Estado = "CONTABILIZADO"
            };

            if (entry.PeriodoId == Guid.Empty)
                return (false, "Error Contable: No existe un periodo contable abierto para la fecha de la nómina.");

            // --- DEBE (GASTOS) ---
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = expenseAccount.Id, Debe = totalDevengado, Haber = 0, Glosa = "Salarios Brutos Devengados" });
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = expenseAccount.Id, Debe = aporteSS, Haber = 0, Glosa = $"Aporte Seg. Social Entidad ({tasaSSPatronal:P1})" });
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = expenseAccount.Id, Debe = impuestoFT, Haber = 0, Glosa = $"Impuesto Fuerza de Trabajo ({tasaFT:P1})" });

            // --- HABER (PASIVOS) ---
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = payableAccount.Id, Debe = 0, Haber = totalRetenciones, Glosa = "Retenciones Legales Trabajadores" });
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = payableAccount.Id, Debe = 0, Haber = aporteSS, Glosa = "Seguridad Social por Pagar (Entidad)" });
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = payableAccount.Id, Debe = 0, Haber = impuestoFT, Glosa = "Impuesto FT por Pagar (Entidad)" });
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = payableAccount.Id, Debe = 0, Haber = totalNeto, Glosa = "Salarios Netos por Pagar (Bancarización)" });

            // 4. Procesar Asiento (Validará partida doble internamente)
            var result = await _accountingService.CreateEntryAsync(entry);
            if (!result.Succeeded) return (false, $"Error al generar comprobante contable: {result.Message}");

            // 5. Actualizar Estado de la Nómina
            period.Estado = "APROBADA";
            period.AprobadoPor = userId;
            period.AsientoId = entry.Id;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, $"Nómina aprobada y contabilizada con éxito. Comprobante #{entry.NumeroComprobante} generado.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Fallo crítico en aprobación: {ex.Message}");
        }
    }
}
