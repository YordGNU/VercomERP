using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IHRService
{
    Task<List<Empleado>> GetActiveEmployeesAsync(Guid entidadId);
    Task<(bool Succeeded, string Message)> RecordAttendanceAsync(RegistroAsistencium attendance);
    Task<(bool Succeeded, string Message)> AddMedicalCertificateAsync(CertificadoMedico certificate);
    Task<decimal> GetAccumulatedVacationsAsync(Guid employeeId);
    Task<List<ContratoLaboral>> GetEmployeeContractsAsync(Guid employeeId);
    Task<(bool Succeeded, string Message)> AccumulateMonthlyVacationsAsync(Guid entidadId, int year, int month, Guid userId);
}

public class HRService : IHRService
{
    private readonly AppDbContext _context;
    private readonly IParametroSistemaService _paramService;

    public HRService(AppDbContext context, IParametroSistemaService paramService)
    {
        _context = context;
        _paramService = paramService;
    }

    public async Task<List<Empleado>> GetActiveEmployeesAsync(Guid entidadId)
    {
        return await _context.Empleados
            .Include(e => e.Cargo)
            .Where(e => e.EntidadId == entidadId && e.Estado == "ACTIVO")
            .ToListAsync();
    }

    public async Task<(bool Succeeded, string Message)> RecordAttendanceAsync(RegistroAsistencium attendance)
    {
        var existing = await _context.RegistroAsistencia
            .AnyAsync(a => a.EmpleadoId == attendance.EmpleadoId && a.Fecha == attendance.Fecha);

        if (existing) return (false, "Ya existe un registro de asistencia para este empleado en la fecha seleccionada.");

        _context.RegistroAsistencia.Add(attendance);
        await _context.SaveChangesAsync();
        return (true, "Asistencia registrada correctamente.");
    }

    public async Task<(bool Succeeded, string Message)> AddMedicalCertificateAsync(CertificadoMedico certificate)
    {
        _context.CertificadoMedicos.Add(certificate);

        for (var date = certificate.FechaInicio; date <= certificate.FechaFin; date = date.AddDays(1))
        {
            var attendance = new RegistroAsistencium
            {
                Id = Guid.NewGuid(),
                EmpleadoId = certificate.EmpleadoId,
                Fecha = date,
                TipoAusenciaId = 1,
                Observaciones = $"Certificado #{certificate.NumeroCertificado}"
            };
            _context.RegistroAsistencia.Add(attendance);
        }

        await _context.SaveChangesAsync();
        return (true, "Certificado médico registrado y días de ausencia generados.");
    }

    public async Task<decimal> GetAccumulatedVacationsAsync(Guid employeeId)
    {
        var saldo = await _context.SaldoVacaciones
            .Where(s => s.EmpleadoId == employeeId)
            .Select(s => s.SaldoActual)
            .FirstOrDefaultAsync() ?? 0;
        return saldo;
    }

    public async Task<List<ContratoLaboral>> GetEmployeeContractsAsync(Guid employeeId)
    {
        return await _context.ContratoLaborals
            .Where(c => c.EmpleadoId == employeeId)
            .OrderByDescending(c => c.FechaInicio)
            .ToListAsync();
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

            var saldo = await _context.SaldoVacaciones
                .FirstOrDefaultAsync(s => s.EmpleadoId == emp.Id && s.Anio == (short)year);

            if (saldo == null)
            {
                saldo = new SaldoVacacione
                {
                    Id = Guid.NewGuid(),
                    EmpleadoId = emp.Id,
                    Anio = (short)year,
                    DiasAcumulados = earned,
                    DiasDisfrutados = 0,
                    DiasCompensados = 0
                };
                _context.SaldoVacaciones.Add(saldo);
            }
            else
            {
                saldo.DiasAcumulados += earned;
            }
        }

        await _context.SaveChangesAsync();
        return (true, "Vacaciones del mes acumuladas correctamente.");
    }
}
