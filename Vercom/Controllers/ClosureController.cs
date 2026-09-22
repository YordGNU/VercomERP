using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Security;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class ClosureController : Controller
{
    private readonly IClosureService _closureService;
    private readonly IIntelligenceService _intelligenceService;
    private readonly IEntidadProvider _entidadProvider;

    public ClosureController(IClosureService closureService, IIntelligenceService intelligenceService, IEntidadProvider entidadProvider)
    {
        _closureService = closureService;
        _intelligenceService = intelligenceService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "CONTABILIDAD.PERIODO.CERRAR")]
    public async Task<IActionResult> Yearly()
    {
        var year = (short)DateTime.Now.Year;
        var vm = await _intelligenceService.GetYearlyClosureContextAsync(_entidadProvider.CurrentEntidadId, year);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.PERIODO.CERRAR")]
    public async Task<IActionResult> ExecuteYearly(short year)
    {
        var result = await _closureService.CloseFiscalYearAsync(_entidadProvider.CurrentEntidadId, year, _entidadProvider.CurrentUsuarioId);

        if (result.Succeeded)
        {
            return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Yearly)) });
        }
        return Json(new { success = false, message = result.Message });
    }
}
