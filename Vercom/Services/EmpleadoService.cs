// Services/EmpleadoService.cs
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;
// Services/IEmpleadoService.cs


public interface IEmpleadoService
{

    Task<EmpleadoExpedienteViewModel> GetExpedienteAsync(Guid empleadoId);

    Task<(bool Success, string Message)> RegistrarVacacionAsync(Guid saldoId, decimal dias, string? observaciones);

    Task<(bool Success, string Message)> DevolverMedioAsync(Guid medioId, DateOnly fechaDevolucion, string? observaciones);
}
public class EmpleadoService : IEmpleadoService
{
    private readonly AppDbContext _context;
    private readonly IHRService _hrservice;

    public EmpleadoService(
        AppDbContext context,
        IHRService hrservice)
    {
        _context = context;
        _hrservice = hrservice;
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

    public async Task<(bool Success, string Message)> RegistrarVacacionAsync(Guid saldoId, decimal dias, string? observaciones)
    {
        return await _hrservice.RecordVacationEnjoymentAsync(saldoId, dias, observaciones);
    }

    public async Task<(bool Success, string Message)> DevolverMedioAsync(Guid medioId, DateOnly fechaDevolucion, string? observaciones)
    {
        var medio = await _context.UtileResponsabilidads.FindAsync(medioId);
        if (medio == null)
            return (false, "Medio no encontrado.");

        if (medio.FechaDevolucion.HasValue)
            return (false, "Este medio ya fue devuelto.");

        medio.FechaDevolucion = fechaDevolucion;
        medio.Observaciones = observaciones;

        await _context.SaveChangesAsync();


        return (true, "Medio devuelto correctamente.");
    }
}