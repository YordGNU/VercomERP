using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize(Policy = "RRHH.ASISTENCIA.VER")]
public class ReporteAsistenciaController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IHRReportService _hrReportService;

    public ReporteAsistenciaController(IAdminService adminService, IHRReportService hrReportService)
    {
        _adminService = adminService;
        _hrReportService = hrReportService;
    }

    [HttpGet]
    public async Task<IActionResult> Mensual(int? year, int? month, Guid? sucursalId)
    {
        var targetYear = year ?? DateTime.Now.Year;
        var targetMonth = month ?? DateTime.Now.Month;

        var vm = await _hrReportService.GetAttendanceMonthlyReportAsync(targetYear, targetMonth, sucursalId);

        ViewBag.Sucursales = new SelectList(await _adminService.GetSucursalesAsync(), "Id", "Nombre", sucursalId);

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ExportMensual(int? year, int? month, Guid? sucursalId)
    {
        var targetYear = year ?? DateTime.Now.Year;
        var targetMonth = month ?? DateTime.Now.Month;

        var vm = await _hrReportService.GetAttendanceMonthlyReportAsync(targetYear, targetMonth, sucursalId);

        var sb = new StringBuilder();
        sb.AppendLine("Empleado;Cargo;Sucursal;Turno;Asistencias;Vacaciones;Incapacidades;Otras Ausencias;Horas Extra;Retardos;Minutos Retardo;Salidas Tempranas;Importe Subsidio");
        foreach (var r in vm.Rows)
        {
            sb.AppendLine(string.Join(";",
                Esc(r.NombreCompleto), Esc(r.Cargo), Esc(r.Sucursal), Esc(r.Turno),
                r.Asistencias, r.Vacaciones, r.Incapacidades, r.OtrasAusencias,
                r.HorasExtra.ToString("0.##"), r.CantidadRetardos, r.MinutosRetardo,
                r.CantidadSalidasTempranas, r.ImporteSubsidio.ToString("0.00")));
        }
        sb.AppendLine(string.Join(";",
            "TOTALES", "", "", "",
            vm.TotalAsistencias, vm.TotalVacaciones, vm.TotalIncapacidades, vm.TotalOtrasAusencias,
            vm.TotalHorasExtra.ToString("0.##"), vm.TotalCantidadRetardos, vm.TotalMinutosRetardo,
            vm.TotalSalidasTempranas, vm.TotalSubsidio.ToString("0.00")));

        var content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(content, "text/csv", $"Asistencia_{targetYear}_{targetMonth:00}.csv");
    }

    private static string Esc(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        var clean = value.Replace("\"", "\"\"");
        return clean.Contains(';') || clean.Contains('"') ? $"\"{clean}\"" : clean;
    }
}
