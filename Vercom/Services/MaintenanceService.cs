using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IMaintenanceService
{
    Task<(bool Succeeded, string Message)> ScheduleMaintenanceAsync(MantenimientoProgramado schedule);
    Task<(bool Succeeded, string Message)> CompleteMaintenanceAsync(Guid scheduleId, decimal actualCost);
    Task<List<MantenimientoProgramado>> GetPendingMaintenancesAsync(Guid entidadId);
}

public class MaintenanceService : IMaintenanceService
{
    private readonly AppDbContext _context;

    public MaintenanceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Succeeded, string Message)> ScheduleMaintenanceAsync(MantenimientoProgramado schedule)
    {
        schedule.Id = Guid.NewGuid();
        schedule.Estado = "PROGRAMADO";
        _context.MantenimientoProgramados.Add(schedule);
        await _context.SaveChangesAsync();
        return (true, "Mantenimiento programado.");
    }

    public async Task<(bool Succeeded, string Message)> CompleteMaintenanceAsync(Guid scheduleId, decimal actualCost)
    {
        var maintenance = await _context.MantenimientoProgramados.FindAsync(scheduleId);
        if (maintenance == null) return (false, "Registro no encontrado.");

        maintenance.Estado = "EJECUTADO";
        maintenance.FechaEjecutada = DateOnly.FromDateTime(DateTime.Now);
        maintenance.Costo = actualCost;

        // Actualizar fecha de última revisión en el equipo
        var equipo = await _context.Equipos.FindAsync(maintenance.EquipoId);
        if (equipo != null)
        {
            equipo.FechaUltimaRevision = maintenance.FechaEjecutada;
        }

        await _context.SaveChangesAsync();
        return (true, "Mantenimiento completado y equipo actualizado.");
    }

    public async Task<List<MantenimientoProgramado>> GetPendingMaintenancesAsync(Guid entidadId)
    {
        return await _context.MantenimientoProgramados
            .Include(m => m.Equipo)
            .Where(m => m.Equipo.EntidadId == entidadId && m.Estado == "PROGRAMADO")
            .OrderBy(m => m.FechaProgramada)
            .ToListAsync();
    }
}
