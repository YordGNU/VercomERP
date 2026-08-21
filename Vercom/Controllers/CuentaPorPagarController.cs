using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.Security;

namespace Vercom.Controllers;

[Authorize]
public class CuentaPorPagarController : Controller
{
    private readonly IReceivablesPayablesService _carteraService;
    private readonly IEntidadProvider _entidadProvider;

    public CuentaPorPagarController(IReceivablesPayablesService carteraService, IEntidadProvider entidadProvider)
    {
        _carteraService = carteraService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "CONTABILIDAD.CXP.VER")]
    public async Task<IActionResult> Index()
    {
        var report = await _carteraService.GetPayablesAgingAsync(_entidadProvider.CurrentEntidadId);
        return View(report);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.PAGO.CREAR")]
    public async Task<IActionResult> RecordPayment(Guid id, decimal amount, string paymentMethod, string? reference)
    {
        var result = await _carteraService.RecordPaymentAsync(id, amount, paymentMethod, reference, _entidadProvider.CurrentUsuarioId);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Index));
    }
}
