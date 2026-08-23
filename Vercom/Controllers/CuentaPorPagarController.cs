using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Security;
using Vercom.Services;
using Vercom.ViewModels;

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
    public async Task<IActionResult> RecordPayment(PaymentRecordViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var result = await _carteraService.RecordPaymentAsync(vm.ItemId, vm.Amount, vm.PaymentMethod, vm.Reference, _entidadProvider.CurrentUsuarioId);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }

        var report = await _carteraService.GetPayablesAgingAsync(_entidadProvider.CurrentEntidadId);
        return View("Index", report);
    }
}
