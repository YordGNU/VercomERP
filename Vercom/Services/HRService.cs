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
    Task<(bool Succeeded, string Message)> AddContractAsync(ContratoLaboral contract);

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

    // Gestión de Tipos de Ausencia
    Task<IEnumerable<TipoAusencium>> GetAbsenceTypesAsync();
    Task<(bool Succeeded, string Message)> CreateAbsenceTypeAsync(TipoAusencium type);
    Task<(bool Succeeded, string Message)> UpdateAbsenceTypeAsync(TipoAusencium type);

    Task<PlantillaStatusViewModel> GetPlantillaStatusAsync();
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

    // Baja Laboral (RF-21)
    Task<TerminateEmployeeViewModel> GetTerminationContextAsync(Guid id);
    Task<(bool Succeeded, string Message)> TerminateEmployeeAsync(Guid id, DateOnly terminationDate, string reason);
}

public class HRService : IHRService
{
    private readonly AppDbContext _context;
    private readonly IParametroSistemaService _paramService;
    private readonly Security.IEntidadProvider _entidadProvider;

    public HRService(AppDbContext context, IParametroSistemaService paramService, Security.IEntidadProvider entidadProvider)
    {
        _context = context;
        _paramService = paramService;
        _entidadProvider = entidadProvider;
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
        return new EmployeeCreateViewModel
        {
            Empleado = existingEmployee ?? new Empleado { Estado = "ACTIVO", FechaIngreso = DateOnly.FromDateTime(DateTime.Now) },
            Cargos = new SelectList(await _context.Cargos.OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre"),
            Sucursales = new SelectList(await _context.Sucursals.OrderBy(s => s.Nombre).ToListAsync(), "Id", "Nombre")
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
            .Select(s => s.SaldoActual)
            .FirstOrDefaultAsync() ?? 0;
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

    public async Task<(bool Succeeded, string Message)> AddContractAsync(ContratoLaboral contract)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var previous = await _context.ContratoLaborals.Where(c => c.EmpleadoId == contract.EmpleadoId && c.Estado == "VIGENTE").ToListAsync();
            foreach (var p in previous) p.Estado = "FINALIZADO";

            contract.Id = Guid.NewGuid();
            contract.CreadoEn = DateTimeOffset.Now;
            _context.ContratoLaborals.Add(contract);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, "Contrato activado.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, ex.Message);
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

    public async Task<PlantillaStatusViewModel> GetPlantillaStatusAsync()
    {
        var entidadId = _entidadProvider.CurrentEntidadId;

        var aprobadas = await _context.PlantillaAprobada
            .Include(p => p.Cargo)
            .Include(p => p.Sucursal)
            .ToListAsync();

        var cubiertas = await _context.ContratoLaborals
            .Include(c => c.Empleado)
            .Where(c => c.Estado == "VIGENTE")
            .GroupBy(c => new { c.CargoId, c.Empleado.SucursalId })
            .Select(g => new { g.Key.CargoId, g.Key.SucursalId, Count = g.Count() })
            .ToListAsync();

        var vm = new PlantillaStatusViewModel();

        foreach (var ap in aprobadas)
        {
            var count = cubiertas.FirstOrDefault(c => c.CargoId == ap.CargoId && c.SucursalId == ap.SucursalId)?.Count ?? 0;
            vm.Rows.Add(new PlantillaRow
            {
                Id = ap.Id,
                Cargo = ap.Cargo.Nombre,
                Sucursal = ap.Sucursal?.Nombre ?? "GLOBAL",
                Aprobadas = ap.PlazasAprobadas,
                Cubiertas = count
            });
        }

        return vm;
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
}
