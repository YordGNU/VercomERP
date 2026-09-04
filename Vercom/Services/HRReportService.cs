using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public class SC408Row
{
    public string Periodo { get; set; } = null!;
    public decimal DiasTrabajados { get; set; }
    public decimal SalarioDevengado { get; set; }
    public int TiempoServicioMeses { get; set; }
}

public class RRHHStatsViewModel
{
    public int TotalActivos { get; set; }
    public int TotalBajas { get; set; }
    public decimal NominaProyectada { get; set; }
    public int ContratosVencer30Dias { get; set; }
}

public interface IHRReportService
{
    Task<List<SC408Row>> GetSC408ReportAsync(Guid employeeId);
    Task<List<SaldoVacacione>> GetVacationSubledgerAsync(Guid entidadId);
    Task<RRHHStatsViewModel> GetGeneralStatsAsync(Guid entidadId);
}

public class HRReportService : IHRReportService
{
    private readonly AppDbContext _context;

    public HRReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RRHHStatsViewModel> GetGeneralStatsAsync(Guid entidadId)
    {
        var limitDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));

        return new RRHHStatsViewModel
        {
            TotalActivos = await _context.Empleados.CountAsync(e => e.Estado == "ACTIVO"),
            TotalBajas = await _context.Empleados.CountAsync(e => e.Estado == "BAJA"),
            NominaProyectada = await _context.ContratoLaborals
                .Where(c => c.Estado == "VIGENTE")
                .SumAsync(c => c.SalarioPactado),
            ContratosVencer30Dias = await _context.ContratoLaborals
                .CountAsync(c => c.Estado == "VIGENTE" && c.FechaFin != null && c.FechaFin <= limitDate)
        };
    }

    public async Task<List<SC408Row>> GetSC408ReportAsync(Guid employeeId)
    {
        var records = await _context.RegistroSalarioTiempoServicios
            .Where(r => r.EmpleadoId == employeeId)
            .OrderBy(r => r.Anio).ThenBy(r => r.Mes)
            .ToListAsync();

        return records.Select(r => new SC408Row
        {
            Periodo = $"{r.Mes}/{r.Anio}",
            DiasTrabajados = r.DiasTrabajados,
            SalarioDevengado = r.SalarioDevengado,
            TiempoServicioMeses = (int)r.TiempoServicioAcumuladoMeses
        }).ToList();
    }

    public async Task<List<SaldoVacacione>> GetVacationSubledgerAsync(Guid entidadId)
    {
        return await _context.SaldoVacaciones
            .Include(s => s.Empleado)
            .Where(s => s.Empleado.EntidadId == entidadId)
            .ToListAsync();
    }
}
