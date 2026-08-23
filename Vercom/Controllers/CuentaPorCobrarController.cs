using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Security;
using Vercom.Services;
using Vercom.ViewModels;

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
    public async Task<IActionResult> RecordCollection(PaymentRecordViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var result = await _carteraService.RecordCollectionAsync(vm.ItemId, vm.Amount, vm.PaymentMethod, vm.Reference, _entidadProvider.CurrentUsuarioId);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }

        var report = await _carteraService.GetReceivablesAgingAsync(_entidadProvider.CurrentEntidadId);
        return View("Index", report);
    }
}
