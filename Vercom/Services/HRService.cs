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
}

public class HRService : IHRService
{
    private readonly AppDbContext _context;

    public HRService(AppDbContext context)
    {
        _context = context;
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

        // Registrar días de ausencia en la tabla de asistencia automáticamente
        for (var date = certificate.FechaInicio; date <= certificate.FechaFin; date = date.AddDays(1))
        {
            var attendance = new RegistroAsistencium
            {
                Id = Guid.NewGuid(),
                EmpleadoId = certificate.EmpleadoId,
                Fecha = date,
                TipoAusenciaId = 1, // ID para "Certificado Médico" (debería estar en Tipos de Ausencia)
                Observaciones = $"Certificado #{certificate.NumeroCertificado}"
            };
            _context.RegistroAsistencia.Add(attendance);
        }

        await _context.SaveChangesAsync();
        return (true, "Certificado médico registrado y días de ausencia generados.");
    }

    public async Task<decimal> GetAccumulatedVacationsAsync(Guid employeeId)
    {
        // En Cuba: 9.09% del tiempo trabajado.
        // Simplificado: Sumar todos los días trabajados y multiplicar por 0.0909
        var diasTrabajados = await _context.RegistroAsistencia
            .CountAsync(a => a.EmpleadoId == employeeId && a.TipoAusenciaId == null);

        // También restamos los días ya disfrutados
        var saldo = await _context.SaldoVacaciones
            .FirstOrDefaultAsync(s => s.EmpleadoId == employeeId);

        return saldo?.SaldoActual ?? (diasTrabajados * 0.0909m);
    }

    public async Task<List<ContratoLaboral>> GetEmployeeContractsAsync(Guid employeeId)
    {
        return await _context.ContratoLaborals
            .Where(c => c.EmpleadoId == employeeId)
            .OrderByDescending(c => c.FechaInicio)
            .ToListAsync();
    }
}
