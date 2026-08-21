using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class TipoAusenciumController : Controller
{
    private readonly IHRService _hrService;

    public TipoAusenciumController(IHRService hrService)
    {
        _hrService = hrService;
    }

    [Authorize(Policy = "RRHH.ASISTENCIA.VER")]
    public async Task<IActionResult> Index()
    {
        var types = await _hrService.GetAbsenceTypesAsync();
        return View(types);
    }

    [Authorize(Policy = "RRHH.ASISTENCIA.REGISTRAR")]
    public IActionResult Create() => View(new TipoAusencium { Remunerada = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.ASISTENCIA.REGISTRAR")]
    public async Task<IActionResult> Create(TipoAusencium type)
    {
        if (ModelState.IsValid)
        {
            var result = await _hrService.CreateAbsenceTypeAsync(type);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        return View(type);
    }
}
