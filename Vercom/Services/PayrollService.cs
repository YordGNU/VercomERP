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
    Task<PayrollPreviewViewModel> GetPayrollPreviewAsync(Guid entidadId, short anio, short mes, Guid? sucursalId = null);
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
            else if (period.Estado != "PRENOMINA" && period.Estado != "CALCULADA")
            {
                return (false, "El periodo de nómina ya está aprobado o cerrado (RNF-22).");
            }

            var existingDetails = _context.NominaDetalles.Where(d => d.PeriodoNominaId == period.Id);
            _context.NominaDetalles.RemoveRange(existingDetails);

            var employees = await _context.Empleados
                .Include(e => e.ContratoLaborals)
                .Include(e => e.Cargo)
                .Where(e => e.EntidadId == entidadId && e.Estado == "ACTIVO")
                .ToListAsync();

            // Parámetros Legales (Iteración 3)
            var ssTasa = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "RET_SS_TRAB") != 0
                ? await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "RET_SS_TRAB") : 0.05m;

            var irpMin = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "UMBRAL_EXENTO_IMP_INGRESOS_PERS") != 0
                ? await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "UMBRAL_EXENTO_IMP_INGRESOS_PERS") : 2500m;

            var irpTasa = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "TASA_IMP_INGRESOS_PERS") != 0
                ? await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "TASA_IMP_INGRESOS_PERS") : 0.03m;

            var tasaRecargoHE = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "TASA_RECARGO_HORA_EXTRA") != 0
                ? await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "TASA_RECARGO_HORA_EXTRA") : 25m;

            // Obtener conceptos por código para evitar hardcoding de IDs
            var conceptos = await _context.ConceptoNominas.Where(c => c.EntidadId == entidadId || c.EntidadId == Guid.Empty).ToListAsync();
            var getConceptId = new Func<string, int?>(code => conceptos.FirstOrDefault(c => c.Codigo == code)?.Id);

            foreach (var emp in employees)
            {
                var contract = emp.ContratoLaborals.FirstOrDefault(c => c.Estado == "VIGENTE");
                if (contract == null) continue;

                // 1. Asistencia y Horas Extra
                var attendanceCount = await _context.RegistroAsistencia
                    .CountAsync(a => a.EmpleadoId == emp.Id && a.Fecha.Year == anio && a.Fecha.Month == mes && a.TipoAusenciaId == null);

                var overtimeHours = await _context.RegistroAsistencia
                    .Where(a => a.EmpleadoId == emp.Id && a.Fecha.Year == anio && a.Fecha.Month == mes)
                    .SumAsync(a => a.HorasExtra);

                // Valor hora con recargo (Iteración 3)
                var valorHoraNormal = contract.SalarioPactado / (contract.JornadaHorasSemana * 4.33m);
                var valorHoraExtra = valorHoraNormal * (1 + (tasaRecargoHE / 100m));
                var montoHoraExtra = Math.Round(overtimeHours * valorHoraExtra, 2);

                // 2. Certificados Médicos (Subsidio SS)
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
                    var start = cert.FechaInicio.Year == anio && cert.FechaInicio.Month == mes ? cert.FechaInicio : new DateOnly(anio, mes, 1);
                    var end = cert.FechaFin.Year == anio && cert.FechaFin.Month == mes ? cert.FechaFin : new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));
                    int daysInMonth = (end.DayNumber - start.DayNumber) + 1;
                    medicalDaysCount += daysInMonth;
                    medicalAmount += Math.Round(dailySalary * daysInMonth * (cert.PorcentajeSubsidio / 100), 2);
                }

                // 3. Vacaciones (Pagadas en el mes)
                var vacationDaysCount = await _context.RegistroAsistencia
                    .Include(a => a.TipoAusencia)
                    .Where(a => a.EmpleadoId == emp.Id && a.Fecha.Year == anio && a.Fecha.Month == mes && a.TipoAusencia != null && (a.TipoAusencia.Codigo == "01" || a.TipoAusencia.Codigo == "VACACIONES"))
                    .CountAsync();
                var vacationAmount = Math.Round(vacationDaysCount * dailySalary, 2);

                var earnedSalary = dailySalary * attendanceCount;
                var variableAmount = await _context.NominaDetalleConceptos
                    .Where(c => c.NominaDetalle.EmpleadoId == emp.Id && c.NominaDetalle.PeriodoNomina.Anio == anio && c.NominaDetalle.PeriodoNomina.Mes == mes)
                    .SumAsync(c => (decimal?)c.Monto) ?? 0;

                var brutoTotal = earnedSalary + montoHoraExtra + variableAmount + medicalAmount + vacationAmount;

                // 4. Retenciones (Iteración 3: IRP sobre excedente)
                var ssRetention = Math.Round(brutoTotal * ssTasa, 2);
                decimal irpAmount = brutoTotal > irpMin ? Math.Round((brutoTotal - irpMin) * (irpTasa / 100m), 2) : 0;

                var detail = new NominaDetalle
                {
                    Id = Guid.NewGuid(),
                    PeriodoNominaId = period.Id,
                    EmpleadoId = emp.Id,
                    DiasTrabajados = attendanceCount + medicalDaysCount + vacationDaysCount,
                    HorasExtra = overtimeHours,
                    SalarioDevengado = brutoTotal,
                    TotalDeducciones = ssRetention + irpAmount,
                    SalarioNeto = brutoTotal - (ssRetention + irpAmount)
                };

                _context.NominaDetalles.Add(detail);

                // Registrar Conceptos (Transparencia)
                void AddConcept(string code, decimal amount) {
                    if (amount == 0) return;
                    var cid = getConceptId(code);
                    if (cid.HasValue) _context.NominaDetalleConceptos.Add(new NominaDetalleConcepto { Id = Guid.NewGuid(), NominaDetalleId = detail.Id, ConceptoId = cid.Value, Monto = amount });
                }

                AddConcept("SAL_BASICO", earnedSalary);
                AddConcept("HORA_EXTRA", montoHoraExtra);
                AddConcept("SUBSIDIO_SS", medicalAmount);
                AddConcept("VACACIONES_PAGO", vacationAmount);
                AddConcept("CONT_SS_TRAB", -ssRetention);
                AddConcept("IMP_INGRESOS_PERS", -irpAmount);
            }

            period.CalculadoEn = DateTimeOffset.Now;
            period.Estado = "CALCULADA";
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Nómina calculada exitosamente con reglas de la Iteración 3.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error en cálculo: {ex.Message}");
        }
    }

    public async Task<PayrollPreviewViewModel> GetPayrollPreviewAsync(Guid entidadId, short anio, short mes, Guid? sucursalId = null)
    {
        var query = _context.Empleados
            .Include(e => e.ContratoLaborals)
            .Include(e => e.Cargo)
            .Include(e => e.Sucursal)
            .Where(e => e.EntidadId == entidadId && e.Estado == "ACTIVO");

        if (sucursalId.HasValue)
        {
            query = query.Where(e => e.SucursalId == sucursalId.Value);
        }

        var employees = await query.ToListAsync();

        var ssTasa = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "RET_SS_TRAB");
        if (ssTasa == 0) ssTasa = 0.05m;

        var irpMin = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "IRP_MIN_EXENTO");
        if (irpMin == 0) irpMin = 3260m;

        var irpTasa = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "IRP_TASA");
        if (irpTasa == 0) irpTasa = 0.03m;

        var vm = new PayrollPreviewViewModel
        {
            Anio = anio,
            Mes = mes,
            SucursalId = sucursalId,
            SucursalNombre = sucursalId.HasValue ? (await _context.Sucursals.FindAsync(sucursalId.Value))?.Nombre : "TODAS"
        };

        foreach (var emp in employees)
        {
            var contract = emp.ContratoLaborals.FirstOrDefault(c => c.Estado == "VIGENTE");
            if (contract == null) continue;

            // Lógica de cálculo (idéntica a la oficial)
            var attendanceCount = await _context.RegistroAsistencia
                .CountAsync(a => a.EmpleadoId == emp.Id && a.Fecha.Year == anio && a.Fecha.Month == mes && a.TipoAusenciaId == null);

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
                var start = cert.FechaInicio.Year == anio && cert.FechaInicio.Month == mes ? cert.FechaInicio : new DateOnly(anio, mes, 1);
                var end = cert.FechaFin.Year == anio && cert.FechaFin.Month == mes ? cert.FechaFin : new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));
                int daysInMonth = (end.DayNumber - start.DayNumber) + 1;
                medicalDaysCount += daysInMonth;
                medicalAmount += (dailySalary * daysInMonth * (cert.PorcentajeSubsidio / 100));
            }

            var overtimeHours = await _context.RegistroAsistencia
                .Where(a => a.EmpleadoId == emp.Id && a.Fecha.Year == anio && a.Fecha.Month == mes)
                .SumAsync(a => a.HorasExtra);

            var overtimeAmount = dailySalary / 8 * overtimeHours * 2;
            var variableAmount = await _context.NominaDetalleConceptos
                .Where(c => c.NominaDetalle.EmpleadoId == emp.Id && c.NominaDetalle.PeriodoNomina.Anio == anio && c.NominaDetalle.PeriodoNomina.Mes == mes)
                .SumAsync(c => (decimal?)c.Monto) ?? 0;

            var brutoTotal = (dailySalary * attendanceCount) + overtimeAmount + variableAmount + medicalAmount;
            var ssRetention = brutoTotal * ssTasa;
            decimal irpAmount = brutoTotal > irpMin ? (brutoTotal - irpMin) * irpTasa : 0;

            vm.Rows.Add(new PayrollPreviewRow
            {
                EmpleadoId = emp.Id,
                NombreCompleto = emp.NombreCompleto,
                Cargo = emp.Cargo.Nombre,
                Sucursal = emp.Sucursal?.Nombre ?? "N/A",
                DiasTrabajados = attendanceCount + medicalDaysCount,
                HorasExtra = overtimeHours,
                Devengado = brutoTotal,
                Deducciones = ssRetention + irpAmount,
                RetencionSS = ssRetention,
                RetencionIRP = irpAmount
            });
        }

        return vm;
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
                .ThenInclude(d => d.NominaDetalleConceptos)
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
            var totalNeto = period.NominaDetalles.Sum(d => d.SalarioNeto);

            // Sumar retenciones por tipo
            var totalRetSS = Math.Abs(period.NominaDetalles.SelectMany(d => d.NominaDetalleConceptos).Where(c => c.ConceptoId == 6).Sum(c => c.Monto));
            var totalRetIRP = Math.Abs(period.NominaDetalles.SelectMany(d => d.NominaDetalleConceptos).Where(c => c.ConceptoId == 7).Sum(c => c.Monto));

            // 1. Obtener Tasas Dinámicas (RF-23)
            var tasaSSPatronal = await _paramService.ObtenerValorNumericoVigenteAsync(period.EntidadId, "TASA_SS_PATRONAL");
            if (tasaSSPatronal == 0) tasaSSPatronal = 0.125m;

            var tasaFT = await _paramService.ObtenerValorNumericoVigenteAsync(period.EntidadId, "TASA_FUERZA_TRAB");
            if (tasaFT == 0) tasaFT = 0.05m;

            var aporteSS = Math.Round(totalDevengado * tasaSSPatronal, 2);
            var impuestoFT = Math.Round(totalDevengado * tasaFT, 2);

            // 2. Obtener Cuentas Contables Configuradas (vía Parámetros o Fallback del listado oficial)
            var ctaGasto = await _paramService.ObtenerValorVigenteAsync(period.EntidadId, "CTA_NOMINA_GASTO") ?? "822";
            var ctaPasivo = await _paramService.ObtenerValorVigenteAsync(period.EntidadId, "CTA_NOMINA_PASIVO") ?? "565.0070";
            var ctaRetSS = await _paramService.ObtenerValorVigenteAsync(period.EntidadId, "CTA_RET_SS_TRAB") ?? "460.0050";
            var ctaRetIRP = await _paramService.ObtenerValorVigenteAsync(period.EntidadId, "CTA_RET_IRP") ?? "460.0060";
            var ctaAporteSS = await _paramService.ObtenerValorVigenteAsync(period.EntidadId, "CTA_APORTE_SS_PAT") ?? "440.0008.001";
            var ctaImpFT = await _paramService.ObtenerValorVigenteAsync(period.EntidadId, "CTA_IMP_FT") ?? "440.0006.001";

            var expenseAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaGasto && c.EntidadId == period.EntidadId);
            var payableAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaPasivo && c.EntidadId == period.EntidadId);
            var ssRetAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaRetSS && c.EntidadId == period.EntidadId);
            var irpRetAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaRetIRP && c.EntidadId == period.EntidadId);
            var ssPatAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaAporteSS && c.EntidadId == period.EntidadId);
            var ftImpAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaImpFT && c.EntidadId == period.EntidadId);

            if (expenseAccount == null || payableAccount == null || ssRetAccount == null || irpRetAccount == null || ssPatAccount == null || ftImpAccount == null)
                return (false, "Error de Configuración: Una o más cuentas contables de nómina no existen en el plan de cuentas de la entidad.");

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

            // --- HABER (PASIVOS / RETENCIONES) ---
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = ssRetAccount.Id, Debe = 0, Haber = totalRetSS, Glosa = "Retención 5% Contrib. Especial Seg. Social" });
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = irpRetAccount.Id, Debe = 0, Haber = totalRetIRP, Glosa = "Retención Impuesto Ingresos Personales" });

            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = ssPatAccount.Id, Debe = 0, Haber = aporteSS, Glosa = "Seguridad Social por Pagar (Entidad)" });
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = ftImpAccount.Id, Debe = 0, Haber = impuestoFT, Glosa = "Impuesto Fuerza Trabajo por Pagar (Entidad)" });
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = payableAccount.Id, Debe = 0, Haber = totalNeto, Glosa = "Salarios Netos por Pagar (Nómina Bancarizada)" });

            // 4. Procesar Asiento (Validará partida doble internamente: DEBE = Gasto + Aportes, HABER = Neto + Retenciones + Aportes)
            // Ajustar el DEBE total para incluir los aportes patronales que son gasto para la empresa
            entry.AsientoDetalles.First(d => d.CuentaId == expenseAccount.Id && d.Glosa == "Salarios Brutos Devengados").Debe = totalDevengado;
            // Los aportes patronales ya están en el DEBE (se agregaron arriba en la sección DEBE (GASTOS))

            var result = await _accountingService.CreateEntryAsync(entry);
            if (!result.Succeeded) return (false, $"Error al generar comprobante contable: {result.Message}");

            // 5. Actualizar Estado de la Nómina (Inmutabilidad RNF-22 vía Trigger DB)
            period.Estado = "CONTABILIZADA";
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
