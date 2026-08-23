using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Security;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class DeclaracionJuradumController : Controller
{
    private readonly ITaxService _taxService;
    private readonly IEntidadProvider _entidadProvider;

    public DeclaracionJuradumController(ITaxService taxService, IEntidadProvider entidadProvider)
    {
        _taxService = taxService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "CONTABILIDAD.DECLARACION.VER")]
    public async Task<IActionResult> Index()
    {
        // Should probably be refactored to IntelligenceService or TaxService for lists
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.DECLARACION.CREAR")]
    public async Task<IActionResult> Create(int tipoObligacionId, Guid periodoId)
    {
        var result = await _taxService.GenerateTaxDeclarationAsync(_entidadProvider.CurrentEntidadId, tipoObligacionId, periodoId);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Index));
    }
}
