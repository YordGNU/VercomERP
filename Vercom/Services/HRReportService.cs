using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.ViewModels;

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
    Task<AttendanceMonthlyReportViewModel> GetAttendanceMonthlyReportAsync(int anio, int mes, Guid? sucursalId = null);
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

    public async Task<AttendanceMonthlyReportViewModel> GetAttendanceMonthlyReportAsync(int anio, int mes, Guid? sucursalId = null)
    {
        var query = _context.Empleados
            .Include(e => e.Cargo)
            .Include(e => e.Sucursal)
            .Include(e => e.TurnoTrabajo)
            .AsQueryable();

        if (sucursalId.HasValue)
        {
            query = query.Where(e => e.SucursalId == sucursalId.Value);
        }

        var empleados = await query.OrderBy(e => e.Apellidos).ToListAsync();
        var employeeIds = empleados.Select(e => e.Id).ToList();

        var registros = await _context.RegistroAsistencia
            .Include(a => a.TipoAusencia)
            .Where(a => a.Fecha.Year == anio && a.Fecha.Month == mes && employeeIds.Contains(a.EmpleadoId))
            .ToListAsync();

        var contratos = await _context.ContratoLaborals
            .Where(c => employeeIds.Contains(c.EmpleadoId) && c.Estado == "VIGENTE")
            .ToListAsync();
        var salarioPorEmpleado = contratos
            .GroupBy(c => c.EmpleadoId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.FechaInicio).First().SalarioPactado);

        var firstDay = new DateOnly(anio, mes, 1);
        var lastDay = new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));

        var certificados = await _context.CertificadoMedicos
            .Where(c => employeeIds.Contains(c.EmpleadoId) && c.FechaInicio <= lastDay && c.FechaFin >= firstDay)
            .ToListAsync();

        const string codigoVacaciones = "01";
        const string codigoIncapacidad = "05";

        var rows = empleados.Select(e =>
        {
            var regs = registros.Where(r => r.EmpleadoId == e.Id).ToList();

            var dailySalary = salarioPorEmpleado.GetValueOrDefault(e.Id) / 24m;
            var subsidio = 0m;
            foreach (var cert in certificados.Where(c => c.EmpleadoId == e.Id))
            {
                var start = cert.FechaInicio < firstDay ? firstDay : cert.FechaInicio;
                var end = cert.FechaFin > lastDay ? lastDay : cert.FechaFin;
                var dias = end.DayNumber - start.DayNumber + 1;
                if (dias > 0)
                {
                    subsidio += Math.Round(dailySalary * dias * (cert.PorcentajeSubsidio / 100m), 2);
                }
            }

            return new AttendanceMonthlyRow
            {
                EmpleadoId = e.Id,
                NombreCompleto = $"{e.Apellidos}, {e.Nombres}",
                Cargo = e.Cargo?.Nombre ?? "-",
                Sucursal = e.Sucursal?.Nombre,
                Turno = e.TurnoTrabajo?.Nombre,
                Asistencias = regs.Count(r => r.TipoAusenciaId == null),
                Vacaciones = regs.Count(r => r.TipoAusencia?.Codigo == codigoVacaciones),
                Incapacidades = regs.Count(r => r.TipoAusencia?.Codigo == codigoIncapacidad),
                OtrasAusencias = regs.Count(r => r.TipoAusenciaId != null
                    && r.TipoAusencia?.Codigo != codigoVacaciones
                    && r.TipoAusencia?.Codigo != codigoIncapacidad),
                HorasExtra = regs.Sum(r => r.HorasExtra),
                CantidadRetardos = regs.Count(r => r.RetardoMinutos > 0),
                MinutosRetardo = regs.Sum(r => r.RetardoMinutos ?? 0),
                CantidadSalidasTempranas = regs.Count(r => r.SalidaTempranaMinutos > 0),
                ImporteSubsidio = subsidio
            };
        }).ToList();

        return new AttendanceMonthlyReportViewModel
        {
            Anio = anio,
            Mes = mes,
            SucursalId = sucursalId,
            Rows = rows
        };
    }
}
