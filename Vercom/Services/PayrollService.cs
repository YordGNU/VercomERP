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
    Task<NominaDetalle?> GetPaySlipAsync(Guid detailId);
}

public class PayrollService : IPayrollService
{
    private readonly AppDbContext _context;
    private readonly IAccountingService _accountingService;
    private readonly IParametroSistemaService _paramService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly INotificationService _notificationService;
    private readonly Security.IEntidadProvider _entidadProvider;

    public PayrollService(AppDbContext context, IAccountingService accountingService, IParametroSistemaService paramService, IServiceScopeFactory scopeFactory, INotificationService notificationService, Security.IEntidadProvider entidadProvider)
    {
        _context = context;
        _accountingService = accountingService;
        _paramService = paramService;
        _scopeFactory = scopeFactory;
        _notificationService = notificationService;
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
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
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

            var irpMin = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "IRP_MIN_EXENTO");
            if (irpMin == 0) irpMin = 3260m;

            var irpTasa = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "IRP_TASA");
            if (irpTasa == 0) irpTasa = 0.03m;

            foreach (var emp in employees)
            {
                var contract = emp.ContratoLaborals.FirstOrDefault(c => c.Estado == "VIGENTE");
                if (contract == null) continue;

                // 1. Días trabajados (Presencia real)
                var attendanceCount = await _context.RegistroAsistencia
                    .CountAsync(a => a.EmpleadoId == emp.Id && a.Fecha.Year == anio && a.Fecha.Month == mes && a.TipoAusenciaId == null);

                // 2. Días de Certificado Médico (Remunerados)
                var medicalDays = await _context.CertificadoMedicos
                    .Where(c => c.EmpleadoId == emp.Id &&
                               ((c.FechaInicio.Year == anio && c.FechaInicio.Month == mes) ||
                                (c.FechaFin.Year == anio && c.FechaFin.Month == mes)))
                    .ToListAsync();

                decimal medicalAmount = 0;
                decimal medicalDaysCount = 0;
                var dailySalary = contract.SalarioPactado / 24;

                foreach (var cert in medicalDays)
                {
                    // Calcular solapamiento con el mes actual
                    var start = cert.FechaInicio.Year == anio && cert.FechaInicio.Month == mes
                                ? cert.FechaInicio
                                : new DateOnly(anio, mes, 1);
                    var end = cert.FechaFin.Year == anio && cert.FechaFin.Month == mes
                              ? cert.FechaFin
                              : new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));

                    int daysInMonth = (end.DayNumber - start.DayNumber) + 1;
                    medicalDaysCount += daysInMonth;
                    medicalAmount += (dailySalary * daysInMonth * (cert.PorcentajeSubsidio / 100));
                }

                var earnedSalary = dailySalary * attendanceCount;

                var overtimeHours = await _context.RegistroAsistencia
                    .Where(a => a.EmpleadoId == emp.Id && a.Fecha.Year == anio && a.Fecha.Month == mes)
                    .SumAsync(a => a.HorasExtra);

                var overtimeAmount = dailySalary / 8 * overtimeHours * 2;

                // RF-22: Pagos por Resultados (Variable)
                var variableAmount = await _context.NominaDetalleConceptos
                    .Where(c => c.NominaDetalle.EmpleadoId == emp.Id && c.NominaDetalle.PeriodoNomina.Anio == anio && c.NominaDetalle.PeriodoNomina.Mes == mes)
                    .SumAsync(c => (decimal?)c.Monto) ?? 0;

                var brutoTotal = earnedSalary + overtimeAmount + variableAmount + medicalAmount;

                // Retenciones Trabajador (RF-23)
                var ssRetention = brutoTotal * ssTasa;

                // Impuesto sobre Ingresos Personales (IRP - Escalado Simplificado para Mipyme)
                decimal irpAmount = 0;
                if (brutoTotal > irpMin)
                {
                    irpAmount = (brutoTotal - irpMin) * irpTasa;
                }

                var detail = new NominaDetalle
                {
                    Id = Guid.NewGuid(),
                    PeriodoNominaId = period.Id,
                    EmpleadoId = emp.Id,
                    DiasTrabajados = attendanceCount + medicalDaysCount,
                    HorasExtra = overtimeHours,
                    SalarioDevengado = brutoTotal,
                    TotalDeducciones = ssRetention + irpAmount,
                    SalarioNeto = brutoTotal - (ssRetention + irpAmount)
                };

                _context.NominaDetalles.Add(detail);
            }

            period.CalculadoEn = DateTimeOffset.Now;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Nómina calculada exitosamente incluyendo subsidios.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error en cálculo: {ex.Message}");
        }
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

            // 6. Acumulación Automática de Vacaciones (9.09%)
            using (var scope = _scopeFactory.CreateScope())
            {
                var hrService = scope.ServiceProvider.GetRequiredService<IHRService>();
                await hrService.AccumulateMonthlyVacationsAsync(period.EntidadId, period.Anio, period.Mes, userId);
            }

            // 7. Registro Histórico SC-4-08 (RF-25)
            foreach (var det in period.NominaDetalles)
            {
                var emp = await _context.Empleados.FindAsync(det.EmpleadoId);
                if (emp == null) continue;

                var periodEndDate = new DateOnly(period.Anio, period.Mes, DateTime.DaysInMonth(period.Anio, period.Mes));
                var totalMonths = ((periodEndDate.Year - emp.FechaIngreso.Year) * 12) + (periodEndDate.Month - emp.FechaIngreso.Month) + 1;

                var historyRecord = new RegistroSalarioTiempoServicio
                {
                    Id = Guid.NewGuid(),
                    EmpleadoId = det.EmpleadoId,
                    Anio = period.Anio,
                    Mes = period.Mes,
                    DiasTrabajados = det.DiasTrabajados,
                    SalarioDevengado = det.SalarioDevengado,
                    TiempoServicioAcumuladoMeses = totalMonths
                };
                _context.RegistroSalarioTiempoServicios.Add(historyRecord);
            }

            // 8. Notificación en Tiempo Real
            await _notificationService.NotifyEntityAsync(period.EntidadId, "Nómina Aprobada", $"Se ha finalizado el pago de {period.NominaDetalles.Count} trabajadores para el mes {period.Mes}.", "success");

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, $"Nómina aprobada, contabilizada y vacaciones acumuladas. Comprobante #{entry.NumeroComprobante}.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Fallo crítico en aprobación: {ex.Message}");
        }
    }

    public async Task<NominaDetalle?> GetPaySlipAsync(Guid detailId)
    {
        return await _context.NominaDetalles
            .Include(d => d.Empleado).ThenInclude(e => e.Cargo)
            .Include(d => d.PeriodoNomina)
            .Include(d => d.NominaDetalleConceptos).ThenInclude(c => c.Concepto)
            .FirstOrDefaultAsync(d => d.Id == detailId);
    }
}
