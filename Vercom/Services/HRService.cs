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
    private readonly IWebHostEnvironment _environment;

    public HRService(AppDbContext context, IParametroSistemaService paramService, Security.IEntidadProvider entidadProvider, IWebHostEnvironment environment, IAdminService adminService)
    {
        _context = context;
        _paramService = paramService;
        _entidadProvider = entidadProvider;
        _environment = environment;
        _adminService = adminService;
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

        return new EmployeeCreateViewModel
        {
            Empleado = existingEmployee ?? new Empleado { Estado = "ACTIVO", FechaIngreso = DateOnly.FromDateTime(DateTime.Now) },
            Cargos = cargos_select_list,
            Sucursales = sucursales_select_list
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateEmployeeAsync(Empleado empleado)
    {
        try
        {
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
        return new EmployeeContractViewModel
        {
            NombreEmpleado = emp != null ? $"{emp.Apellidos}, {emp.Nombres}" : "Desconocido",
            Contrato = new ContratoLaboral { EmpleadoId = employeeId, FechaInicio = DateOnly.FromDateTime(DateTime.Now), Estado = "VIGENTE" },
            Cargos = new SelectList(await _context.Cargos.ToListAsync(), "Id", "Nombre"),
            TiposContrato = new SelectList(new[] { "PRUEBA", "DETERMINADO", "INDETERMINADO" })
        };
    }

    public async Task<(bool Succeeded, string Message)> AddContractAsync(ContratoLaboral contract, IFormFile? document)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Manejar archivo físico si existe
            if (document != null)
            {
                var webRoot = _environment.WebRootPath;
                if (string.IsNullOrEmpty(webRoot))
                {
                    webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                var uploadsFolder = Path.Combine(webRoot, "uploads", "contracts");

                try
                {
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                }
                catch (Exception dirEx)
                {
                    return (false, "Error de permisos en el servidor: No se pudo crear la carpeta de adjuntos. " + dirEx.Message);
                }

                var extension = Path.GetExtension(document.FileName).ToLower();
                var allowedExtensions = new[] { ".doc", ".docx", ".pdf" };
                if (!allowedExtensions.Contains(extension))
                    return (false, "Formato de archivo no permitido. Solo se aceptan documentos Word o PDF.");

                var fileName = $"Contrato_{contract.EmpleadoId}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await document.CopyToAsync(fileStream);
                }

                contract.DocumentoUrl = "/uploads/contracts/" + fileName;
            }

            // 2. Finalizar contratos previos
            var previous = await _context.ContratoLaborals
                .Where(c => c.EmpleadoId == contract.EmpleadoId && c.Estado == "VIGENTE")
                .ToListAsync();

            foreach (var p in previous) p.Estado = "FINALIZADO";

            // 3. Registrar el nuevo contrato
            if (contract.Id == Guid.Empty) contract.Id = Guid.NewGuid();
            contract.CreadoEn = DateTimeOffset.Now;
            if (string.IsNullOrEmpty(contract.Estado)) contract.Estado = "VIGENTE";

            _context.ContratoLaborals.Add(contract);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, document != null ? "Contrato registrado con documento digital." : "Contrato registrado exitosamente.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, "Fallo crítico al guardar contrato: " + ex.Message);
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
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            certificate.Id = Guid.NewGuid();
            certificate.CreadoEn = DateTimeOffset.Now;
            _context.CertificadoMedicos.Add(certificate);

            for (var date = certificate.FechaInicio; date <= certificate.FechaFin; date = date.AddDays(1))
            {
                var attendance = new RegistroAsistencium
                {
                    Id = Guid.NewGuid(),
                    EmpleadoId = certificate.EmpleadoId,
                    Fecha = date,
                    TipoAusenciaId = 1,
                    Observaciones = $"AUT: Certificado #{certificate.NumeroCertificado}"
                };
                _context.RegistroAsistencia.Add(attendance);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, "Certificado y ausencias registradas.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
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
            .Where(a => a.Fecha == targetDate && employeeIds.Contains(a.EmpleadoId))
            .ToDictionaryAsync(a => a.EmpleadoId);

        return new AttendanceConsoleViewModel
        {
            Date = targetDate,
            TiposAusencia = await _context.TipoAusencia.ToListAsync(),
            Rows = employees.Select(e => new AttendanceRow
            {
                EmpleadoId = e.Id,
                NombreCompleto = $"{e.Apellidos}, {e.Nombres}",
                Cargo = e.Cargo.Nombre,
                Record = records.ContainsKey(e.Id) ? records[e.Id] : new RegistroAsistencium
                {
                    EmpleadoId = e.Id,
                    Fecha = targetDate,
                    HoraEntrada = new TimeOnly(8, 0),
                    HoraSalida = new TimeOnly(17, 0)
                }
            }).ToList()
        };
    }

    public async Task<(bool Succeeded, string Message)> SaveAttendanceConsoleAsync(List<RegistroAsistencium> logs, Guid userId)
    {
        try
        {
            foreach (var log in logs)
            {
                var existing = await _context.RegistroAsistencia.FirstOrDefaultAsync(a => a.EmpleadoId == log.EmpleadoId && a.Fecha == log.Fecha);
                if (existing != null)
                {
                    existing.HoraEntrada = log.HoraEntrada; existing.HoraSalida = log.HoraSalida;
                    existing.HorasExtra = log.HorasExtra; existing.TipoAusenciaId = log.TipoAusenciaId;
                    existing.Observaciones = log.Observaciones;
                }
                else
                {
                    log.Id = Guid.NewGuid();
                    log.RegistradoPor = userId;
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
        using var transaction = await _context.Database.BeginTransactionAsync();
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
            await transaction.CommitAsync();
            return (true, "Baja laboral procesada correctamente.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, ex.Message);
        }
    }

    public async Task<EmpleadoExpedienteViewModel> GetExpedienteAsync(Guid empleadoId)
    {
        // Cargar empleado con todas las relaciones necesarias
        var empleado = await _context.Empleados
            .Include(e => e.Cargo)
            .Include(e => e.Sucursal)
            .Include(e => e.ContratoLaborals)
            .Include(e => e.RegistroAsistencia)
                .ThenInclude(r => r.TipoAusencia)
            .Include(e => e.CertificadoMedicos)
            .Include(e => e.UtileResponsabilidades)
            .FirstOrDefaultAsync(e => e.Id == empleadoId);

        if (empleado == null)
            throw new KeyNotFoundException($"Empleado {empleadoId} no encontrado.");

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
