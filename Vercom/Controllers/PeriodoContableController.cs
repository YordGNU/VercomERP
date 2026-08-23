using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Security;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class PeriodoContableController : Controller
{
    private readonly IAccountingService _accountingService;
    private readonly IEntidadProvider _entidadProvider;

    public PeriodoContableController(IAccountingService accountingService, IEntidadProvider entidadProvider)
    {
        _accountingService = accountingService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.VER")]
    public async Task<IActionResult> Index()
    {
        var periods = await _accountingService.GetPeriodsAsync();
        return View(periods);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.PERIODO.CERRAR")]
    public async Task<IActionResult> Close(Guid id)
    {
        var result = await _accountingService.ClosePeriodAsync(id, _entidadProvider.CurrentUsuarioId);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Index));
    }
}
