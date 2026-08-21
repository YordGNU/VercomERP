using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface IHRService
{
    // Lectura
    Task<IEnumerable<Empleado>> GetEmployeesAsync();
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
    Task<AttendanceConsoleViewModel> GetAttendanceConsoleAsync(DateTime date);
    Task<(bool Succeeded, string Message)> SaveAttendanceConsoleAsync(List<RegistroAsistencium> logs, Guid userId);

    // Gestión de Cargos (RF-26)
    Task<IEnumerable<Cargo>> GetCargosAsync();
    Task<(bool Succeeded, string Message)> CreateCargoAsync(Cargo cargo);
    Task<(bool Succeeded, string Message)> UpdateCargoAsync(Cargo cargo);

    // Gestión de Tipos de Ausencia
    Task<IEnumerable<TipoAusencium>> GetAbsenceTypesAsync();
    Task<(bool Succeeded, string Message)> CreateAbsenceTypeAsync(TipoAusencium type);
    Task<(bool Succeeded, string Message)> UpdateAbsenceTypeAsync(TipoAusencium type);

    Task<PlantillaStatusViewModel> GetPlantillaStatusAsync();
    Task<(bool Succeeded, string Message)> CreatePlantillaEntryAsync(PlantillaAprobadum entry);

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

    public async Task<IEnumerable<Empleado>> GetEmployeesAsync()
    {
        return await _context.Empleados
            .Include(e => e.Cargo)
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
            TiposContrato = new SelectList(new[] { "INDETERMINADO", "DETERMINADO", "ADIESTRAMIENTO", "APRENDIZAJE" })
        };
    }

    public async Task<(bool Succeeded, string Message)> AddContractAsync(ContratoLaboral contract)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var previous = await _context.ContratoLaborals.Where(c => c.EmpleadoId == contract.EmpleadoId && c.Estado == "VIGENTE").ToListAsync();
            foreach (var p in previous) p.Estado = "HISTORICO";

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

    public async Task<AttendanceConsoleViewModel> GetAttendanceConsoleAsync(DateTime date)
    {
        var targetDate = DateOnly.FromDateTime(date);
        var employees = await _context.Empleados.Include(e => e.Cargo).Where(e => e.Estado == "ACTIVO").OrderBy(e => e.Apellidos).ToListAsync();
        var records = await _context.RegistroAsistencia.Where(a => a.Fecha == targetDate).ToDictionaryAsync(a => a.EmpleadoId);

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
                    HoraEntrada = new TimeOnly(8,0),
                    HoraSalida = new TimeOnly(17,0)
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
                Cargo = ap.Cargo.Nombre,
                Sucursal = ap.Sucursal?.Nombre ?? "GLOBAL",
                Aprobadas = ap.PlazasAprobadas,
                Cubiertas = count
            });
        }

        return vm;
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

            // Mark contracts as historical
            var contracts = await _context.ContratoLaborals.Where(c => c.EmpleadoId == id && c.Estado == "VIGENTE").ToListAsync();
            foreach (var c in contracts) c.Estado = "HISTORICO";

            // Compensation of vacations (liquidation)
            var saldo = await _context.SaldoVacaciones
                .FirstOrDefaultAsync(s => s.EmpleadoId == id && s.Anio == (short)DateTime.Now.Year);

            if (saldo != null)
            {
                saldo.DiasCompensados += (decimal)saldo.SaldoActual;
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
