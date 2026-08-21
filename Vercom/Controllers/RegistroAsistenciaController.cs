using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.Security;

namespace Vercom.Controllers;

[Authorize]
public class RegistroAsistenciaController : Controller
{
    private readonly IHRService _hrService;
    private readonly IEntidadProvider _entidadProvider;

    public RegistroAsistenciaController(IHRService hrService, IEntidadProvider entidadProvider)
    {
        _hrService = hrService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "RRHH.ASISTENCIA.VER")]
    public async Task<IActionResult> Index()
    {
        // Por simplicidad, el Index podría mostrar un resumen o redirigir a la consola
        return RedirectToAction(nameof(Console));
    }

    [HttpGet]
    [Authorize(Policy = "RRHH.ASISTENCIA.REGISTRAR")]
    public async Task<IActionResult> Console(DateTime? date)
    {
        var targetDate = date ?? DateTime.Now;
        var vm = await _hrService.GetAttendanceConsoleAsync(targetDate);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.ASISTENCIA.REGISTRAR")]
    public async Task<IActionResult> SaveConsole(List<RegistroAsistencium> logs, DateTime date)
    {
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
