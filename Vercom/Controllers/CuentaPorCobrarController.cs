using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.Security;

namespace Vercom.Controllers;

[Authorize]
public class CuentaPorCobrarController : Controller
{
    private readonly IReceivablesPayablesService _carteraService;
    private readonly IEntidadProvider _entidadProvider;

    public CuentaPorCobrarController(IReceivablesPayablesService carteraService, IEntidadProvider entidadProvider)
    {
        _carteraService = carteraService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "CONTABILIDAD.CXC.VER")]
    public async Task<IActionResult> Index()
    {
        var report = await _carteraService.GetReceivablesAgingAsync(_entidadProvider.CurrentEntidadId);
        return View(report);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.PAGO.CREAR")]
    public async Task<IActionResult> RecordCollection(Guid id, decimal amount, string paymentMethod, string? reference)
    {
        var result = await _carteraService.RecordCollectionAsync(id, amount, paymentMethod, reference, _entidadProvider.CurrentUsuarioId);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Index));
    }
}
