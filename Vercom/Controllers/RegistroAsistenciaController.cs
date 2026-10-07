using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Security;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class RegistroAsistenciaController : Controller
{
    private readonly IHRService _hrService;
    private readonly IAdminService _adminService;
    private readonly IEntidadProvider _entidadProvider;

    public RegistroAsistenciaController(IHRService hrService, IAdminService adminService, IEntidadProvider entidadProvider)
    {
        _hrService = hrService;
        _adminService = adminService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "RRHH.ASISTENCIA.VER")]
    public async Task<IActionResult> Index()
    {
        return RedirectToAction(nameof(Console));
    }

    [HttpGet]
    [Authorize(Policy = "RRHH.ASISTENCIA.REGISTRAR")]
    public async Task<IActionResult> Console(DateTime? date, Guid? sucursalId, Guid? cargoId, string? search)
    {
        var targetDate = date ?? DateTime.Now;
        var vm = await _hrService.GetAttendanceConsoleAsync(targetDate, sucursalId, cargoId, search);

        ViewBag.SucursalId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _adminService.GetSucursalesAsync(), "Id", "Nombre", sucursalId);
        ViewBag.CargoId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _hrService.GetCargosAsync(), "Id", "Nombre", cargoId);
        ViewBag.CurrentSearch = search;

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.ASISTENCIA.REGISTRAR")]
    public async Task<IActionResult> SaveConsole(List<RegistroAsistencium> logs, DateTime date)
    {
        var targetDate = DateOnly.FromDateTime(date);
        if (logs.Any(log => log.Fecha != targetDate))
        {
            TempData["Error"] = "La fecha de una o más filas no coincide con la jornada seleccionada.";
            return RedirectToAction(nameof(Console), new { date });
        }

        var result = await _hrService.SaveAttendanceConsoleAsync(logs, _entidadProvider.CurrentUsuarioId);
        if (result.Succeeded)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }
        return RedirectToAction(nameof(Console), new { date });
    }
}
