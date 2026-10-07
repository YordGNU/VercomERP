using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface IHRService
{
    // Lectura
    Task<IEnumerable<Empleado>> GetEmployeesAsync(string? search = null, Guid? cargoId = null, Guid? sucursalId = null, string? estado = null, DateOnly? desde = null, DateOnly? hasta = null);
    Task<Empleado?> GetEmployeeByIdAsync(Guid id);
    Task<EmployeeCreateViewModel> GetEmployeeCreateContextAsync(Empleado? existingEmployee = null);
    Task<List<ContratoLaboral>> GetEmployeeContractsAsync(Guid employeeId);
    Task<decimal> GetAccumulatedVacationsAsync(Guid employeeId);

    // Escritura
    Task<(bool Succeeded, string Message)> CreateEmployeeAsync(Empleado empleado);
    Task<(bool Succeeded, string Message)> CreateEmployeeWithContractAsync(Empleado empleado, ContratoLaboral contract, IFormFile? document);
    Task<(bool Succeeded, string Message)> UpdateEmployeeAsync(Empleado empleado);
    Task<List<Empleado>> GetActiveEmployeesAsync(Guid entidadId);
    Task<(bool Succeeded, string Message)> AccumulateMonthlyVacationsAsync(Guid entidadId, int year, int month, Guid userId);

    // Gestión de Contratos (RF-20)
    Task<EmployeeContractViewModel> GetContractCreateContextAsync(Guid employeeId);
    Task<(bool Succeeded, string Message)> AddContractAsync(ContratoLaboral contract, IFormFile? document);

    // Gestión de Certificados Médicos (RF-21)
    Task<MedicalCertificateViewModel> GetMedicalCertificateCreateContextAsync(Guid employeeId);
    Task<(bool Succeeded, string Message)> AddMedicalCertificateAsync(CertificadoMedico certificate);

    // Consola de Asistencia (RF-21)
    Task<AttendanceConsoleViewModel> GetAttendanceConsoleAsync(DateTime date, Guid? sucursalId = null, Guid? cargoId = null, string? search = null);
    Task<(bool Succeeded, string Message)> SaveAttendanceConsoleAsync(List<RegistroAsistencium> logs, Guid userId);

    // Gestión de Cargos (RF-26)
    Task<IEnumerable<Cargo>> GetCargosAsync();
    Task<Cargo?> GetCargoByIdAsync(Guid id);
    Task<(bool Succeeded, string Message)> CreateCargoAsync(Cargo cargo);
    Task<(bool Succeeded, string Message)> UpdateCargoAsync(Cargo cargo);
    Task<(bool Succeeded, string Message)> DeleteCargoAsync(Guid id);

    // Gestión de Tipos de Ausencia
    Task<IEnumerable<TipoAusencium>> GetAbsenceTypesAsync();
    Task<(bool Succeeded, string Message)> CreateAbsenceTypeAsync(TipoAusencium type);
    Task<(bool Succeeded, string Message)> UpdateAbsenceTypeAsync(TipoAusencium type);

    Task<PlantillaAprobadum?> GetPlantillaEntryByIdAsync(Guid id);
    Task<(bool Succeeded, string Message)> CreatePlantillaEntryAsync(PlantillaAprobadum entry);
    Task<(bool Succeeded, string Message)> UpdatePlantillaEntryAsync(PlantillaAprobadum entry);
    Task<(bool Succeeded, string Message)> DeletePlantillaEntryAsync(Guid id);

    // Gestión de Útiles (Responsabilidad Material)
    Task<IEnumerable<UtileResponsabilidad>> GetUtilesAsync();
    Task<UtileResponsabilidad?> GetUtileByIdAsync(Guid id);
    Task<List<UtileResponsabilidad>> GetUtilesByEmployeeAsync(Guid employeeId);
    Task<(bool Succeeded, string Message)> AssignUtileAsync(UtileResponsabilidad utile);
    Task<(bool Succeeded, string Message)> ReturnUtileAsync(Guid id, DateOnly returnDate, string? observations);

    // Gestión de Vacaciones
    Task<(bool Succeeded, string Message)> RecordVacationEnjoymentAsync(Guid saldoId, decimal days, string? observations);

    // Baja Laboral (RF-21)
    Task<TerminateEmployeeViewModel> GetTerminationContextAsync(Guid id);
    Task<(bool Succeeded, string Message)> TerminateEmployeeAsync(Guid id, DateOnly terminationDate, string reason);
    Task<EmpleadoExpedienteViewModel?> GetExpedienteAsync(Guid value);
    Task<PlantillaStatusViewModel?> GetPlantillaStatusAsync(Guid? sucursalId, string? search);
}

public class HRService : IHRService
{
    private readonly AppDbContext _context;
    private readonly IParametroSistemaService _paramService;
    private readonly IAdminService _adminService;
    private readonly Security.IEntidadProvider _entidadProvider;
    private readonly IFileStorageService _fileStorage;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<HRService> _logger;

    public HRService(AppDbContext context, IParametroSistemaService paramService, Security.IEntidadProvider entidadProvider, IWebHostEnvironment environment, IAdminService adminService, IHttpContextAccessor httpContextAccessor, IFileStorageService fileStorageService, ILogger<HRService> logger)
    {
        _context = context;
        _paramService = paramService;
        _entidadProvider = entidadProvider;
        _environment = environment;
        _fileStorage = fileStorageService;
        _adminService = adminService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<IEnumerable<Empleado>> GetEmployeesAsync(string? search = null, Guid? cargoId = null, Guid? sucursalId = null, string? estado = null, DateOnly? desde = null, DateOnly? hasta = null)
    {
        var query = _context.Empleados
            .Include(e => e.Cargo)
            .Include(e => e.Sucursal)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e => e.Nombres.Contains(search) || e.Apellidos.Contains(search) || e.CarnetIdentidad.Contains(search));
        }

        if (cargoId.HasValue)
        {
            query = query.Where(e => e.CargoId == cargoId.Value);
        }

        if (sucursalId.HasValue)
        {
            query = query.Where(e => e.SucursalId == sucursalId.Value);
        }

        if (!string.IsNullOrEmpty(estado))
        {
            query = query.Where(e => e.Estado == estado);
        }

        if (desde.HasValue)
        {
            query = query.Where(e => e.FechaIngreso >= desde.Value);
        }

        if (hasta.HasValue)
        {
            query = query.Where(e => e.FechaIngreso <= hasta.Value);
        }

        return await query
            .OrderBy(e => e.Apellidos)
            .ToListAsync();
    }

    public async Task<Empleado?> GetEmployeeByIdAsync(Guid id)
    {
        return await _context.Empleados
            .Include(e => e.Cargo)
            .Include(e => e.ContratoLaborals)
            .Include(e => e.RegistroAsistencia).ThenInclude(a => a.TipoAusencia)
            .Include(e => e.SaldoVacaciones)
            .Include(e => e.CertificadoMedicos)
            .Include(e => e.UtileResponsabilidades)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<EmployeeCreateViewModel> GetEmployeeCreateContextAsync(Empleado? existingEmployee = null)
    {
        var cargos = await GetCargosAsync();
        var cargos_select_list = new SelectList(cargos.Select(c => new { Id = c.Id, DisplayText = $"{c.Codigo} - {c.Nombre}" }), "Id", "DisplayText");

        var sucursales = await _adminService.GetSucursalesAsync();
        var sucursales_select_list = new SelectList(sucursales.Select(c => new { Id = c.Id, DisplayText = $"{c.Codigo} - {c.Nombre}" }), "Id", "DisplayText");

        var turnos = await _context.TurnoTrabajos
            .Where(t => t.Activo && t.EntidadId == _entidadProvider.CurrentEntidadId)
            .OrderBy(t => t.HoraEntrada)
            .ToListAsync();
        var turnos_select_list = new SelectList(
            turnos.Select(t => new { t.Id, DisplayText = $"{t.Nombre} ({t.HoraEntrada:HH:mm} - {t.HoraSalida:HH:mm})" }),
            "Id", "DisplayText");

        return new EmployeeCreateViewModel
        {
            Empleado = existingEmployee ?? new Empleado { Estado = "ACTIVO", FechaIngreso = DateOnly.FromDateTime(DateTime.Now) },
            Contrato = new ContratoLaboral { FechaInicio = DateOnly.FromDateTime(DateTime.Now), Estado = "VIGENTE", JornadaHorasSemana = 44 },
            Cargos = cargos_select_list,
            Sucursales = sucursales_select_list,
            TiposContrato = new SelectList(new[] { "PRUEBA", "DETERMINADO", "INDETERMINADO" }),
            Turnos = turnos_select_list
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateEmployeeWithContractAsync(Empleado empleado, ContratoLaboral contract, IFormFile? document)
    {
        // Limpiar navegación para evitar conflictos con EF (Iteración 3)
        contract.Empleado = null;
        contract.Cargo = null;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // --- Sincronización de Datos ---
            if (empleado.CargoId == Guid.Empty && contract.CargoId != Guid.Empty)
                empleado.CargoId = contract.CargoId;

            // 1. Crear Empleado
            var empResult = await CreateEmployeeAsync(empleado);
            if (!empResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return empResult;
            }

            // 2. Vincular Contrato
            contract.EmpleadoId = empleado.Id;
            if (contract.CargoId == Guid.Empty) contract.CargoId = empleado.CargoId;

            var contractResult = await AddContractAsync(contract, document);
            if (!contractResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return contractResult;
            }

            await transaction.CommitAsync();
            return (true, "Expediente y primer contrato registrados correctamente.");
        }
        catch (Exception ex)
        {
            if (_context.Database.CurrentTransaction != null)
                await transaction.RollbackAsync();

            var innerMsg = ex.InnerException != null ? " | Detalle: " + ex.InnerException.Message : "";
            _logger.LogError(ex, "Fallo en registro unificado de empleado {CI}", empleado.CarnetIdentidad);
            return (false, "Error en registro unificado: " + ex.Message + innerMsg);
        }
    }

    public async Task<(bool Succeeded, string Message)> CreateEmployeeAsync(Empleado empleado)
    {
        try
        {
            if (empleado.TurnoTrabajoId.HasValue && !await _context.TurnoTrabajos.AnyAsync(t =>
                    t.Id == empleado.TurnoTrabajoId.Value && t.EntidadId == _entidadProvider.CurrentEntidadId && t.Activo))
                return (false, "El turno seleccionado no existe, está inactivo o pertenece a otra entidad.");

            empleado.Id = Guid.NewGuid();
            empleado.EntidadId = _entidadProvider.CurrentEntidadId;
            empleado.CreadoEn = DateTimeOffset.Now;

            if (string.IsNullOrEmpty(empleado.Estado))
                empleado.Estado = "ACTIVO";

            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();
            return (true, "Empleado creado correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al crear empleado: {ex.Message}");
        }
    }

    public async Task<(bool Succeeded, string Message)> UpdateEmployeeAsync(Empleado empleado)
    {
        try
        {
            var existing = await _context.Empleados.FindAsync(empleado.Id);
            if (existing == null) return (false, "No existe.");

            if (empleado.TurnoTrabajoId.HasValue && !await _context.TurnoTrabajos.AnyAsync(t =>
                    t.Id == empleado.TurnoTrabajoId.Value && t.EntidadId == _entidadProvider.CurrentEntidadId && t.Activo))
                return (false, "El turno seleccionado no existe, está inactivo o pertenece a otra entidad.");

            _context.Entry(existing).CurrentValues.SetValues(empleado);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;

            await _context.SaveChangesAsync();
            return (true, "Actualizado correctamente.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<List<Empleado>> GetActiveEmployeesAsync(Guid entidadId)
    {
        return await _context.Empleados
            .Include(e => e.Cargo)
            .Where(e => e.Estado == "ACTIVO")
            .ToListAsync();
    }

    public async Task<decimal> GetAccumulatedVacationsAsync(Guid employeeId)
    {
        return await _context.SaldoVacaciones
            .Where(s => s.EmpleadoId == employeeId)
            .SumAsync(s => s.SaldoActual) ?? 0;
    }

    public async Task<List<ContratoLaboral>> GetEmployeeContractsAsync(Guid employeeId)
    {
        return await _context.ContratoLaborals
            .Where(c => c.EmpleadoId == employeeId)
            .OrderByDescending(c => c.FechaInicio)
            .ToListAsync();
    }

    public async Task<EmployeeContractViewModel> GetContractCreateContextAsync(Guid employeeId)
    {
        var emp = await _context.Empleados.FindAsync(employeeId);
        if (emp == null)
        {
            return new EmployeeContractViewModel
            {
                NombreEmpleado = "Desconocido",
                Contrato = new ContratoLaboral
                {
                    EmpleadoId = employeeId,
                    FechaInicio = DateOnly.FromDateTime(DateTime.Now),
                    Estado = "VIGENTE"
                },
                Cargos = new SelectList(Enumerable.Empty<object>(), "Id", "DisplayText"),
                TiposContrato = new SelectList(new[] { "PRUEBA", "DETERMINADO", "INDETERMINADO" })
            };
        }

        var nuevoContrato = new ContratoLaboral
        {
            EmpleadoId = employeeId,
            CargoId = emp.CargoId,
            FechaInicio = DateOnly.FromDateTime(DateTime.Now),
            Estado = "VIGENTE"
        };

        var cargos = await _context.Cargos
            .Where(c => c.EntidadId == emp.EntidadId)
            .OrderBy(c => c.Codigo)
            .Select(c => new
            {
                c.Id,
                DisplayText = c.Codigo + " - " + c.Nombre
            })
            .ToListAsync();

        return new EmployeeContractViewModel
        {
            NombreEmpleado = $"{emp.Apellidos}, {emp.Nombres}",
            Contrato = nuevoContrato,
            Cargos = new SelectList(cargos, "Id", "DisplayText", emp.CargoId),
            TiposContrato = new SelectList(new[] { "PRUEBA", "DETERMINADO", "INDETERMINADO" })
        };
    }

    public async Task<(bool Succeeded, string Message)> AddContractAsync(ContratoLaboral contract, IFormFile? document)
    {
        using var transaction = _context.Database.CurrentTransaction == null
            ? await _context.Database.BeginTransactionAsync()
            : null;

        string? uploadedRelativePath = null; // Para limpiar si falla la transacción

        try
        {
            // ============================================================
            // 1. VALIDACIÓN DE CUPO EN PLANTILLA (RF-26)
            // ============================================================
            if (contract.Id == Guid.Empty)
            {
                var sucursalId = contract.Empleado?.SucursalId
                    ?? (await _context.Empleados.FindAsync(contract.EmpleadoId))?.SucursalId;

                var disponibles = await _context.PlantillaAprobada
                    .Where(p => p.EntidadId == _entidadProvider.CurrentEntidadId
                             && p.CargoId == contract.CargoId
                             && p.SucursalId == sucursalId)
                    .Select(p => p.PlazasAprobadas)
                    .FirstOrDefaultAsync();

                var cubiertas = await _context.ContratoLaborals
                    .CountAsync(c => c.CargoId == contract.CargoId
                                  && c.Empleado.SucursalId == sucursalId
                                  && c.Estado == "VIGENTE");

                if (disponibles > 0 && cubiertas >= disponibles)
                {
                    return (false, $"No hay plazas disponibles para este cargo en la sucursal seleccionada (Cupo: {disponibles}).");
                }
            }

            // ============================================================
            // 2. GUARDAR DOCUMENTO ADJUNTO (usando el servicio)
            // ============================================================
            if (document != null && document.Length > 0)
            {
                var uploadResult = await _fileStorage.SaveFileAsync(
                    file: document,
                    subFolder: "contracts",
                    prefix: $"contrato_{contract.EmpleadoId}");

                if (!uploadResult.Success)
                    return (false, uploadResult.Message);

                contract.DocumentoUrl = uploadResult.RelativePath;
                uploadedRelativePath = uploadResult.RelativePath;

                _logger.LogInformation(
                    "Documento de contrato subido: {Path} para empleado {EmpleadoId}",
                    uploadedRelativePath, contract.EmpleadoId);
            }

            // ============================================================
            // 3. FINALIZAR CONTRATOS PREVIOS
            // ============================================================
            var previous = await _context.ContratoLaborals
                .Where(c => c.EmpleadoId == contract.EmpleadoId && c.Estado == "VIGENTE")
                .ToListAsync();

            foreach (var p in previous)
                p.Estado = "FINALIZADO";

            // ============================================================
            // 4. REGISTRAR EL NUEVO CONTRATO
            // ============================================================
            if (contract.Id == Guid.Empty)
                contract.Id = Guid.NewGuid();

            contract.CreadoEn = DateTimeOffset.Now;

            if (string.IsNullOrEmpty(contract.Estado))
                contract.Estado = "VIGENTE";

            _context.ContratoLaborals.Add(contract);
            await _context.SaveChangesAsync();


            if (transaction != null)
                await transaction.CommitAsync();

            return (true, document != null
                ? "Contrato registrado con documento digital."
                : "Contrato registrado exitosamente.");
        }
        catch (DbUpdateException dbEx)
        {
            if (transaction != null)
                await transaction.RollbackAsync();

            if (uploadedRelativePath != null)
                await _fileStorage.DeleteFileAsync(uploadedRelativePath);

            var innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
            if (dbEx.InnerException?.InnerException != null)
                innerMsg += " | Sub-detalle: " + dbEx.InnerException.InnerException.Message;

            _logger.LogError(dbEx, "Error de BD al guardar contrato para empleado {EmpleadoId}", contract.EmpleadoId);
            return (false, "Error de base de datos: " + innerMsg);
        }
        catch (Exception ex)
        {
            if (transaction != null)
                await transaction.RollbackAsync();

            if (uploadedRelativePath != null)
                await _fileStorage.DeleteFileAsync(uploadedRelativePath);

            var errorDetail = ex.InnerException?.Message ?? ex.Message;
            _logger.LogError(ex, "Error inesperado al guardar contrato para empleado {EmpleadoId}", contract.EmpleadoId);
            return (false, "Fallo crítico al guardar contrato: " + errorDetail);
        }
    }

    public async Task<MedicalCertificateViewModel> GetMedicalCertificateCreateContextAsync(Guid employeeId)
    {
        var emp = await _context.Empleados.FindAsync(employeeId);
        return new MedicalCertificateViewModel
        {
            NombreEmpleado = emp != null ? $"{emp.Apellidos}, {emp.Nombres}" : "Desconocido",
            Certificado = new CertificadoMedico { EmpleadoId = employeeId, FechaInicio = DateOnly.FromDateTime(DateTime.Now), PorcentajeSubsidio = 100 }
        };
    }

    public async Task<(bool Succeeded, string Message)> AddMedicalCertificateAsync(CertificadoMedico certificate)
    {
        if (certificate.FechaFin < certificate.FechaInicio)
            return (false, "La fecha final del certificado no puede ser anterior a la fecha inicial.");
        if (certificate.PorcentajeSubsidio < 0 || certificate.PorcentajeSubsidio > 100)
            return (false, "El porcentaje del subsidio debe estar entre 0 y 100.");
        if (!await _context.Empleados.AnyAsync(e => e.Id == certificate.EmpleadoId))
            return (false, "El trabajador no existe o pertenece a otra entidad.");

        var overlaps = await _context.CertificadoMedicos.AnyAsync(c =>
            c.EmpleadoId == certificate.EmpleadoId &&
            c.FechaInicio <= certificate.FechaFin && c.FechaFin >= certificate.FechaInicio);
        if (overlaps)
            return (false, "El trabajador ya tiene un certificado médico que se solapa con ese período.");

        var incapacidad = await _context.TipoAusencia.FirstOrDefaultAsync(t => t.Codigo == "05");
        if (incapacidad == null)
            return (false, "No está configurado el tipo de ausencia 05 (Incapacidad Temporal).");

        using var transaction = _context.Database.CurrentTransaction == null ? await _context.Database.BeginTransactionAsync() : null;
        try
        {
            certificate.Id = Guid.NewGuid();
            certificate.CreadoEn = DateTimeOffset.Now;
            _context.CertificadoMedicos.Add(certificate);

            for (var date = certificate.FechaInicio; date <= certificate.FechaFin; date = date.AddDays(1))
            {
                var attendance = await _context.RegistroAsistencia
                    .FirstOrDefaultAsync(a => a.EmpleadoId == certificate.EmpleadoId && a.Fecha == date);
                if (attendance == null)
                {
                    attendance = new RegistroAsistencium
                    {
                        Id = Guid.NewGuid(),
                        EmpleadoId = certificate.EmpleadoId,
                        Fecha = date
                    };
                    _context.RegistroAsistencia.Add(attendance);
                }

                attendance.TipoAusenciaId = incapacidad.Id;
                attendance.HoraEntrada = null;
                attendance.HoraSalida = null;
                attendance.HorasExtra = 0;
                attendance.RetardoMinutos = null;
                attendance.SalidaTempranaMinutos = null;
                var marker = $"AUT: Certificado #{certificate.NumeroCertificado}";
                if (string.IsNullOrWhiteSpace(attendance.Observaciones))
                    attendance.Observaciones = marker;
                else if (!attendance.Observaciones.Contains(marker, StringComparison.Ordinal))
                    attendance.Observaciones = $"{attendance.Observaciones}; {marker}";
            }

            await _context.SaveChangesAsync();
            if (transaction != null) await transaction.CommitAsync();
            return (true, "Certificado y ausencias registradas.");
        }
        catch (Exception ex)
        {
            if (transaction != null) await transaction.RollbackAsync();
            return (false, ex.Message);
        }
    }

    public async Task<(bool Succeeded, string Message)> AccumulateMonthlyVacationsAsync(Guid entidadId, int year, int month, Guid userId)
    {
        var employees = await GetActiveEmployeesAsync(entidadId);
        var factor = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "FACTOR_VAC");
        if (factor == 0) factor = 0.0909m;

        foreach (var emp in employees)
        {
            var daysWorked = await _context.RegistroAsistencia
                .CountAsync(a => a.EmpleadoId == emp.Id && a.Fecha.Year == year && a.Fecha.Month == month && a.TipoAusenciaId == null);

            var earned = daysWorked * factor;
            var saldo = await _context.SaldoVacaciones.FirstOrDefaultAsync(s => s.EmpleadoId == emp.Id && s.Anio == (short)year);

            if (saldo == null)
            {
                saldo = new SaldoVacacione { Id = Guid.NewGuid(), EmpleadoId = emp.Id, Anio = (short)year, DiasAcumulados = earned };
                _context.SaldoVacaciones.Add(saldo);
            }
            else
            {
                saldo.DiasAcumulados += earned;
            }
        }

        await _context.SaveChangesAsync();
        return (true, "Vacaciones del mes acumuladas.");
    }

    public async Task<AttendanceConsoleViewModel> GetAttendanceConsoleAsync(DateTime date, Guid? sucursalId = null, Guid? cargoId = null, string? search = null)
    {
        var targetDate = DateOnly.FromDateTime(date);

        var query = _context.Empleados
            .Include(e => e.Cargo)
            .Include(e => e.TurnoTrabajo)
            .Where(e => e.Estado == "ACTIVO")
            .AsQueryable();

        if (sucursalId.HasValue)
        {
            query = query.Where(e => e.SucursalId == sucursalId.Value);
        }

        if (cargoId.HasValue)
        {
            query = query.Where(e => e.CargoId == cargoId.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e => e.Nombres.Contains(search) || e.Apellidos.Contains(search) || e.CarnetIdentidad.Contains(search));
        }

        var employees = await query.OrderBy(e => e.Apellidos).ToListAsync();
        var employeeIds = employees.Select(e => e.Id).ToList();

        var records = await _context.RegistroAsistencia
            .Include(a => a.TurnoTrabajo)
            .Where(a => a.Fecha == targetDate && employeeIds.Contains(a.EmpleadoId))
            .ToDictionaryAsync(a => a.EmpleadoId);

        var certificados = await _context.CertificadoMedicos
            .Where(c => employeeIds.Contains(c.EmpleadoId) && c.FechaInicio <= targetDate && c.FechaFin >= targetDate)
            .ToListAsync();
        var certificadoPorEmpleado = certificados
            .GroupBy(c => c.EmpleadoId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.FechaInicio).First());

        var tiposAusencia = await _context.TipoAusencia.OrderBy(t => t.Nombre).ToListAsync();
        var incapacidad = tiposAusencia.FirstOrDefault(t => t.Codigo == "05");

        return new AttendanceConsoleViewModel
        {
            Date = targetDate,
            TiposAusencia = tiposAusencia,
            IncapacidadTipoAusenciaId = incapacidad?.Id,
            Rows = employees.Select(e =>
            {
                var isNew = !records.TryGetValue(e.Id, out var record);
                record ??= new RegistroAsistencium { EmpleadoId = e.Id, Fecha = targetDate };

                var turno = record.TurnoTrabajo ?? e.TurnoTrabajo;

                if (isNew && record.TipoAusenciaId == null)
                {
                    record.TurnoTrabajoId = turno?.Id;
                    record.HoraEntrada = turno?.HoraEntrada ?? new TimeOnly(8, 0);
                    record.HoraSalida = turno?.HoraSalida ?? new TimeOnly(17, 0);
                }

                var certificado = certificadoPorEmpleado.GetValueOrDefault(e.Id);
                var esIncapacidad = certificado != null && incapacidad != null;
                if (esIncapacidad)
                {
                    record.TipoAusenciaId = incapacidad!.Id;
                    record.HoraEntrada = null;
                    record.HoraSalida = null;
                }

                var (retardo, temprana) = esIncapacidad
                    ? (null, null)
                    : ComputeJornadaMetrics(turno, targetDate, record.HoraEntrada, record.HoraSalida);

                return new AttendanceRow
                {
                    EmpleadoId = e.Id,
                    NombreCompleto = $"{e.Apellidos}, {e.Nombres}",
                    Cargo = e.Cargo.Nombre,
                    TurnoTrabajoId = turno?.Id,
                    TurnoNombre = turno?.Nombre,
                    TurnoHoraEntrada = turno?.HoraEntrada,
                    TurnoHoraSalida = turno?.HoraSalida,
                    EsNocturno = turno?.EsNocturno ?? false,
                    RetardoMinutos = retardo,
                    SalidaTempranaMinutos = temprana,
                    EsIncapacidad = esIncapacidad,
                    PorcentajeSubsidio = certificado?.PorcentajeSubsidio,
                    Record = record
                };
            }).ToList()
        };
    }

    private static (int? Retardo, int? SalidaTemprana) ComputeJornadaMetrics(TurnoTrabajo? turno, DateOnly fecha, TimeOnly? entrada, TimeOnly? salida)
    {
        if (turno == null) return (null, null);

        var crossesMidnight = turno.HoraSalida <= turno.HoraEntrada;
        var scheduledIn = fecha.ToDateTime(turno.HoraEntrada);
        var scheduledOut = fecha.ToDateTime(turno.HoraSalida);
        if (crossesMidnight) scheduledOut = scheduledOut.AddDays(1);

        int? retardo = null;
        if (entrada.HasValue)
        {
            var actualIn = ResolveShiftTime(fecha, entrada.Value, scheduledIn, crossesMidnight);
            var lateMinutes = (actualIn - scheduledIn).TotalMinutes;
            retardo = lateMinutes > turno.ToleranciaMinutos ? (int)Math.Round(lateMinutes) : 0;
        }

        int? temprana = null;
        if (salida.HasValue)
        {
            var actualOut = ResolveShiftTime(fecha, salida.Value, scheduledOut, crossesMidnight);
            var earlyMinutes = (scheduledOut - actualOut).TotalMinutes;
            temprana = earlyMinutes > 0 ? (int)Math.Round(earlyMinutes) : 0;
        }

        return (retardo, temprana);
    }

    private static DateTime ResolveShiftTime(DateOnly fecha, TimeOnly hora, DateTime scheduledTime, bool crossesMidnight)
    {
        var sameDay = fecha.ToDateTime(hora);
        if (!crossesMidnight) return sameDay;

        var nextDay = sameDay.AddDays(1);
        return Math.Abs((nextDay - scheduledTime).Ticks) < Math.Abs((sameDay - scheduledTime).Ticks)
            ? nextDay
            : sameDay;
    }

    public async Task<(bool Succeeded, string Message)> SaveAttendanceConsoleAsync(List<RegistroAsistencium> logs, Guid userId)
    {
        try
        {
            if (logs == null || logs.Count == 0) return (true, "Sin cambios que guardar.");

            if (logs.GroupBy(l => new { l.EmpleadoId, l.Fecha }).Any(g => g.Count() > 1))
                return (false, "La solicitud contiene filas duplicadas para el mismo trabajador y fecha.");

            if (logs.Any(l => l.HorasExtra < 0 || l.HorasExtra > 99.99m))
                return (false, "Las horas extra deben estar entre 0 y 99.99.");

            var employeeIds = logs.Select(l => l.EmpleadoId).Distinct().ToList();
            var employees = await _context.Empleados
                .Where(e => employeeIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id);
            if (employees.Count != employeeIds.Count)
                return (false, "Uno o más trabajadores no existen o pertenecen a otra entidad.");

            var existingRecords = await _context.RegistroAsistencia
                .Where(a => employeeIds.Contains(a.EmpleadoId) && logs.Select(l => l.Fecha).Contains(a.Fecha))
                .ToDictionaryAsync(a => new { a.EmpleadoId, a.Fecha });

            var turnoIds = logs.Where(l => l.TurnoTrabajoId.HasValue).Select(l => l.TurnoTrabajoId!.Value).Distinct().ToList();
            var turnos = await _context.TurnoTrabajos
                .Where(t => turnoIds.Contains(t.Id) && t.EntidadId == _entidadProvider.CurrentEntidadId)
                .ToDictionaryAsync(t => t.Id);

            if (turnos.Count != turnoIds.Count)
                return (false, "Uno o más turnos no existen o pertenecen a otra entidad.");

            var certificates = await _context.CertificadoMedicos
                .Where(c => employeeIds.Contains(c.EmpleadoId) && logs.Min(l => l.Fecha) <= c.FechaFin && logs.Max(l => l.Fecha) >= c.FechaInicio)
                .ToListAsync();
            var incapacidad = await _context.TipoAusencia.FirstOrDefaultAsync(t => t.Codigo == "05");
            if (certificates.Count > 0 && incapacidad == null)
                return (false, "No está configurado el tipo de ausencia 05 (Incapacidad Temporal).");

            foreach (var log in logs)
            {
                var employee = employees[log.EmpleadoId];
                var key = new { log.EmpleadoId, log.Fecha };
                existingRecords.TryGetValue(key, out var existing);

                var expectedTurnoId = existing?.TurnoTrabajoId ?? employee.TurnoTrabajoId;
                if (log.TurnoTrabajoId != expectedTurnoId)
                    return (false, "El turno enviado no coincide con el turno asignado al trabajador para esa jornada.");

                var certificate = certificates.FirstOrDefault(c => c.EmpleadoId == log.EmpleadoId && c.FechaInicio <= log.Fecha && c.FechaFin >= log.Fecha);
                if (certificate != null)
                {
                    log.TipoAusenciaId = incapacidad!.Id;
                    log.HoraEntrada = null;
                    log.HoraSalida = null;
                    log.HorasExtra = 0;
                    var marker = $"AUT: Certificado #{certificate.NumeroCertificado}";
                    if (string.IsNullOrWhiteSpace(log.Observaciones))
                        log.Observaciones = marker;
                    else if (!log.Observaciones.Contains(marker, StringComparison.Ordinal))
                        log.Observaciones = $"{log.Observaciones}; {marker}";
                }

                var turno = log.TurnoTrabajoId.HasValue && turnos.TryGetValue(log.TurnoTrabajoId.Value, out var t) ? t : null;
                var (retardo, temprana) = log.TipoAusenciaId.HasValue
                    ? (null, null)
                    : ComputeJornadaMetrics(turno, log.Fecha, log.HoraEntrada, log.HoraSalida);
                log.RetardoMinutos = retardo;
                log.SalidaTempranaMinutos = temprana;

                if (existing != null)
                {
                    existing.HoraEntrada = log.HoraEntrada; existing.HoraSalida = log.HoraSalida;
                    existing.HorasExtra = log.HorasExtra; existing.TipoAusenciaId = log.TipoAusenciaId;
                    existing.Observaciones = log.Observaciones;
                    existing.TurnoTrabajoId = log.TurnoTrabajoId;
                    existing.RetardoMinutos = log.RetardoMinutos;
                    existing.SalidaTempranaMinutos = log.SalidaTempranaMinutos;
                }
                else
                {
                    log.Id = Guid.NewGuid();
                    log.RegistradoPor = userId;
                    log.Empleado = null!;
                    log.TurnoTrabajo = null;
                    log.TipoAusencia = null;
                    _context.RegistroAsistencia.Add(log);
                }
            }
            await _context.SaveChangesAsync();
            return (true, "Asistencia guardada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<Cargo>> GetCargosAsync()
    {
        return await _context.Cargos.OrderBy(c => c.Nombre).ToListAsync();
    }

    public async Task<Cargo?> GetCargoByIdAsync(Guid id)
    {
        return await _context.Cargos
            .Include(c => c.Empleados).ThenInclude(e => e.Sucursal)
            .Include(c => c.PlantillaAprobada).ThenInclude(p => p.Sucursal)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<(bool Succeeded, string Message)> CreateCargoAsync(Cargo cargo)
    {
        try
        {
            cargo.Id = Guid.NewGuid();
            cargo.EntidadId = _entidadProvider.CurrentEntidadId;
            _context.Cargos.Add(cargo);
            await _context.SaveChangesAsync();
            return (true, "Cargo creado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateCargoAsync(Cargo cargo)
    {
        try
        {
            var existing = await _context.Cargos.FindAsync(cargo.Id);
            if (existing == null) return (false, "No existe.");
            _context.Entry(existing).CurrentValues.SetValues(cargo);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;
            await _context.SaveChangesAsync();
            return (true, "Cargo actualizado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> DeleteCargoAsync(Guid id)
    {
        try
        {
            var cargo = await _context.Cargos
                .Include(c => c.Empleados)
                .Include(c => c.PlantillaAprobada)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cargo == null) return (false, "Cargo no encontrado.");

            if (cargo.Empleados.Any())
                return (false, "No se puede eliminar el cargo porque tiene trabajadores vinculados.");

            if (cargo.PlantillaAprobada.Any())
                return (false, "No se puede eliminar el cargo porque tiene plazas aprobadas en plantilla.");

            _context.Cargos.Remove(cargo);
            await _context.SaveChangesAsync();
            return (true, "Cargo eliminado correctamente.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<TipoAusencium>> GetAbsenceTypesAsync()
    {
        return await _context.TipoAusencia.OrderBy(a => a.Nombre).ToListAsync();
    }

    public async Task<(bool Succeeded, string Message)> CreateAbsenceTypeAsync(TipoAusencium type)
    {
        try
        {
            _context.TipoAusencia.Add(type);
            await _context.SaveChangesAsync();
            return (true, "Tipo de ausencia creado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateAbsenceTypeAsync(TipoAusencium type)
    {
        try
        {
            _context.Update(type);
            await _context.SaveChangesAsync();
            return (true, "Tipo de ausencia actualizado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<PlantillaStatusViewModel> GetPlantillaStatusAsync(Guid? sucursalId = null, string? search = null)
    {
        var entidadId = _entidadProvider.CurrentEntidadId;
        var isMaster = _entidadProvider.IsMaster;
        var hoy = DateOnly.FromDateTime(DateTime.Now);

        // ============================================================
        // 1. Obtener plazas aprobadas (con filtros y vigencia)
        // ============================================================
        var aprobadasQuery = _context.PlantillaAprobada
            .Include(p => p.Cargo)
            .Include(p => p.Sucursal)
            .Include(p => p.Entidad)
            .AsQueryable();

        // Si es Master, ignorar filtros de entidad (ver todas)
        if (!isMaster)
            aprobadasQuery = aprobadasQuery.Where(p => p.EntidadId == entidadId);

        // Filtrar por sucursal (si se proporciona)
        if (sucursalId.HasValue)
            aprobadasQuery = aprobadasQuery.Where(p => p.SucursalId == sucursalId.Value);

        // Filtrar por búsqueda en nombre del cargo
        if (!string.IsNullOrEmpty(search))
            aprobadasQuery = aprobadasQuery.Where(p => p.Cargo.Nombre.Contains(search));

        // Solo plazas vigentes (fecha actual dentro del rango)
        aprobadasQuery = aprobadasQuery.Where(p => p.VigenteDesde <= hoy && (p.VigenteHasta == null || p.VigenteHasta >= hoy));

        var aprobadas = await aprobadasQuery.ToListAsync();

        // ============================================================
        // 2. Obtener contratos vigentes (para contar cubiertas)
        // ============================================================
        var cubiertasQuery = _context.ContratoLaborals
            .Include(c => c.Empleado)
            .Where(c => c.Estado == "VIGENTE");

        if (!isMaster)
            cubiertasQuery = cubiertasQuery.Where(c => c.Empleado.EntidadId == entidadId);

        // Agrupar por CargoId y SucursalId del empleado
        var cubiertas = await cubiertasQuery
            .GroupBy(c => new { c.CargoId, c.Empleado.SucursalId })
            .Select(g => new { g.Key.CargoId, g.Key.SucursalId, Count = g.Count() })
            .ToListAsync();

        // ============================================================
        // 3. Construir las filas del ViewModel
        // ============================================================
        var rows = new List<PlantillaRow>();

        foreach (var ap in aprobadas)
        {
            var count = cubiertas
                .FirstOrDefault(c => c.CargoId == ap.CargoId && c.SucursalId == ap.SucursalId)
                ?.Count ?? 0;

            rows.Add(new PlantillaRow
            {
                Id = ap.Id,
                CargoId = ap.CargoId,
                SucursalId = ap.SucursalId,
                Cargo = ap.Cargo?.Codigo + " - " + ap.Cargo?.Nombre ?? "Sin cargo",
                Sucursal = ap.Sucursal?.Codigo + " - " + ap.Sucursal?.Nombre ?? "GLOBAL",
                EntidadNombre = ap.Entidad?.NombreComercial ?? "Desconocida",
                Aprobadas = ap.PlazasAprobadas,
                Cubiertas = count
            });
        }

        // ============================================================
        // 4. Calcular totales
        // ============================================================
        var totalAprobadas = rows.Sum(r => r.Aprobadas);
        var totalCubiertas = rows.Sum(r => r.Cubiertas);
        var vacantes = totalAprobadas - totalCubiertas;
        var porcentajeCobertura = totalAprobadas > 0
            ? (decimal)totalCubiertas / totalAprobadas * 100
            : 0;

        // ============================================================
        // 5. Crear y retornar el ViewModel
        // ============================================================
        return new PlantillaStatusViewModel
        {
            Rows = rows.OrderBy(r => r.Cargo).ThenBy(r => r.Sucursal).ToList(),
            TotalPlazasAprobadas = totalAprobadas,
            TotalPlazasCubiertas = totalCubiertas,
            TotalVacantes = vacantes,
            PorcentajeCoberturaTotal = decimal.Round(porcentajeCobertura, 2),
            FechaCalculo = DateTime.Now
        };
    }
    public async Task<PlantillaAprobadum?> GetPlantillaEntryByIdAsync(Guid id)
    {
        return await _context.PlantillaAprobada
            .Include(p => p.Cargo)
            .Include(p => p.Sucursal)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(bool Succeeded, string Message)> CreatePlantillaEntryAsync(PlantillaAprobadum entry)
    {
        try
        {
            entry.Id = Guid.NewGuid();
            entry.EntidadId = _entidadProvider.CurrentEntidadId;
            if (entry.VigenteDesde == default) entry.VigenteDesde = DateOnly.FromDateTime(DateTime.Now);

            _context.PlantillaAprobada.Add(entry);
            await _context.SaveChangesAsync();
            return (true, "Plaza aprobada registrada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdatePlantillaEntryAsync(PlantillaAprobadum entry)
    {
        try
        {
            var existing = await _context.PlantillaAprobada.FindAsync(entry.Id);
            if (existing == null) return (false, "No existe.");

            _context.Entry(existing).CurrentValues.SetValues(entry);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;

            await _context.SaveChangesAsync();
            return (true, "Plaza actualizada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> DeletePlantillaEntryAsync(Guid id)
    {
        try
        {
            var entry = await _context.PlantillaAprobada.FindAsync(id);
            if (entry == null) return (false, "No existe.");

            _context.PlantillaAprobada.Remove(entry);
            await _context.SaveChangesAsync();
            return (true, "Plaza eliminada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<UtileResponsabilidad>> GetUtilesAsync()
    {
        return await _context.UtileResponsabilidads
            .Include(u => u.Empleado)
            .OrderByDescending(u => u.FechaEntrega)
            .ToListAsync();
    }

    public async Task<UtileResponsabilidad?> GetUtileByIdAsync(Guid id)
    {
        return await _context.UtileResponsabilidads
            .Include(u => u.Empleado)
            .Include(u => u.Entidad)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<List<UtileResponsabilidad>> GetUtilesByEmployeeAsync(Guid employeeId)
    {
        return await _context.UtileResponsabilidads
            .Where(u => u.EmpleadoId == employeeId)
            .OrderByDescending(u => u.FechaEntrega)
            .ToListAsync();
    }

    public async Task<(bool Succeeded, string Message)> AssignUtileAsync(UtileResponsabilidad utile)
    {
        try
        {
            utile.Id = Guid.NewGuid();
            utile.EntidadId = _entidadProvider.CurrentEntidadId;
            utile.CreadoEn = DateTimeOffset.Now;
            if (utile.FechaEntrega == default) utile.FechaEntrega = DateOnly.FromDateTime(DateTime.Now);

            _context.UtileResponsabilidads.Add(utile);
            await _context.SaveChangesAsync();
            return (true, "Útil/Medio asignado correctamente.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> ReturnUtileAsync(Guid id, DateOnly returnDate, string? observations)
    {
        try
        {
            var utile = await _context.UtileResponsabilidads.FindAsync(id);
            if (utile == null) return (false, "Registro no encontrado.");

            utile.FechaDevolucion = returnDate;
            if (!string.IsNullOrEmpty(observations))
            {
                utile.Observaciones = (utile.Observaciones ?? "") + " | DEVOLUCIÓN: " + observations;
            }

            await _context.SaveChangesAsync();
            return (true, "Devolución registrada correctamente.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> RecordVacationEnjoymentAsync(Guid saldoId, decimal days, string? observations)
    {
        try
        {
            var saldo = await _context.SaldoVacaciones.FindAsync(saldoId);
            if (saldo == null) return (false, "Registro no encontrado.");

            if (days <= 0) return (false, "Los días deben ser mayores que cero.");
            if (days > saldo.SaldoActual) return (false, $"Saldo insuficiente ({saldo.SaldoActual:N1} disponibles).");

            saldo.DiasDisfrutados += days;
            await _context.SaveChangesAsync();
            return (true, $"Se registraron {days} días de disfrute.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<TerminateEmployeeViewModel> GetTerminationContextAsync(Guid id)
    {
        var emp = await _context.Empleados
            .Include(e => e.Cargo)
            .Include(e => e.ContratoLaborals)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (emp == null) throw new Exception("Empleado no encontrado.");

        var contract = emp.ContratoLaborals.FirstOrDefault(c => c.Estado == "VIGENTE");
        var vacations = await GetAccumulatedVacationsAsync(id);

        return new TerminateEmployeeViewModel
        {
            EmpleadoId = id,
            NombreCompleto = $"{emp.Apellidos}, {emp.Nombres}",
            Cargo = emp.Cargo.Nombre,
            FechaIngreso = emp.FechaIngreso,
            VacacionesPendientes = vacations,
            SalarioPactado = contract?.SalarioPactado ?? 0
        };
    }

    public async Task<(bool Succeeded, string Message)> TerminateEmployeeAsync(Guid id, DateOnly terminationDate, string reason)
    {
        using var transaction = _context.Database.CurrentTransaction == null ? await _context.Database.BeginTransactionAsync() : null;
        try
        {
            var emp = await _context.Empleados.FindAsync(id);
            if (emp == null) return (false, "Empleado no encontrado.");

            emp.Estado = "BAJA";
            emp.FechaBaja = terminationDate;
            emp.MotivoBaja = reason;

            // Mark contracts as finalizado
            var contracts = await _context.ContratoLaborals.Where(c => c.EmpleadoId == id && c.Estado == "VIGENTE").ToListAsync();
            foreach (var c in contracts) c.Estado = "FINALIZADO";

            // Compensation of vacations (liquidation)
            var saldo = await _context.SaldoVacaciones
                .FirstOrDefaultAsync(s => s.EmpleadoId == id && s.Anio == (short)DateTime.Now.Year);

            if (saldo != null)
            {
                saldo.DiasCompensados += (decimal)(saldo.SaldoActual ?? 0);
            }

            // Desactivar usuario vinculado si existe
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.EsEmpleadoId == id);
            if (user != null)
            {
                user.Activo = false;
            }

            await _context.SaveChangesAsync();
            if (transaction != null) await transaction.CommitAsync();
            return (true, "Baja laboral procesada correctamente.");
        }
        catch (Exception ex)
        {
            if (transaction != null) await transaction.RollbackAsync();
            return (false, ex.Message);
        }
    }

    public async Task<EmpleadoExpedienteViewModel> GetExpedienteAsync(Guid empleadoId)
    {
        // Cargar empleado con todas las relaciones necesarias
        var query = _context.Empleados
            .Include(e => e.Cargo)
            .Include(e => e.Sucursal)
            .Include(e => e.ContratoLaborals)
            .Include(e => e.RegistroAsistencia)
                .ThenInclude(r => r.TipoAusencia)
            .Include(e => e.CertificadoMedicos)
            .Include(e => e.UtileResponsabilidades)
            .AsQueryable();

        var empleado = await query.FirstOrDefaultAsync(e => e.Id == empleadoId);

        if (empleado == null)
            throw new KeyNotFoundException($"Empleado {empleadoId} no encontrado.");

        // --- RNF-20: Restricción de Privacidad Médica (Iteración 3) ---
        // Si el usuario no tiene rol RRHH o ADMIN, ocultamos diagnósticos sensibles
        var userRoles = _httpContextAccessor.HttpContext?.User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(r => r.Value).ToList() ?? new List<string>();
        bool canViewDiagnosis = userRoles.Contains("RRHH") || userRoles.Contains("ADMIN") || userRoles.Contains("DIRECCION") || userRoles.Contains("MASTER");

        if (!canViewDiagnosis)
        {
            foreach (var cert in empleado.CertificadoMedicos)
            {
                cert.DiagnosticoCie = "[ACCESO RESTRINGIDO]";
            }
        }

        var hoy = DateOnly.FromDateTime(DateTime.Now);

        // Calcular antigüedad
        var antiguedad = hoy.Year - empleado.FechaIngreso.Year;
        if (hoy < empleado.FechaIngreso.AddYears(antiguedad)) antiguedad--;

        // Calcular edad
        var edad = 0;
        if (empleado.FechaNacimiento != null)
        {
            edad = hoy.Year - empleado.FechaNacimiento.Year;
            if (hoy < empleado.FechaNacimiento.AddYears(edad)) edad--;
        }

        // Obtener saldo de vacaciones del año actual
        var saldoVacaciones = await _context.SaldoVacaciones
            .FirstOrDefaultAsync(s => s.EmpleadoId == empleadoId && s.Anio == hoy.Year);

        // Obtener últimas nóminas (12 meses)
        var nominas = await _context.NominaDetalles
            .Include(n => n.PeriodoNomina)
            .Where(n => n.EmpleadoId == empleadoId)
            .OrderByDescending(n => n.PeriodoNomina.Anio)
            .ThenByDescending(n => n.PeriodoNomina.Mes)
            .Take(12)
            .ToListAsync();

        // Construir ViewModel
        var vm = new EmpleadoExpedienteViewModel
        {
            Empleado = empleado,
            AntiguedadAnios = antiguedad,
            Edad = edad,
            SaldoVacaciones = saldoVacaciones?.SaldoActual ?? 0,
            ContratosVigentes = empleado.ContratoLaborals.Count(c => c.Estado == "VIGENTE"),
            Contratos = empleado.ContratoLaborals.OrderByDescending(c => c.FechaInicio).ToList(),
            Asistencias = empleado.RegistroAsistencia.OrderByDescending(a => a.Fecha).Take(30).ToList(),
            Certificados = empleado.CertificadoMedicos.OrderByDescending(c => c.FechaInicio).ToList(),
            Medios = empleado.UtileResponsabilidades.OrderByDescending(u => u.FechaEntrega).ToList(),
            Nominas = nominas,
            TotalContratos = empleado.ContratoLaborals.Count,
            DiasVacacionesTomados = (int)(saldoVacaciones?.DiasDisfrutados ?? 0)
        };

        return vm;
    }

}
