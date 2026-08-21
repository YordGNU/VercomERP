using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class DevolucionVentumController : Controller
{
    private readonly ICommercialService _commercialService;

    public DevolucionVentumController(ICommercialService commercialService)
    {
        _commercialService = commercialService;
    }

    [Authorize(Policy = "COMERCIAL.FACTURA_VENTA.VER")]
    public async Task<IActionResult> Index()
    {
        var items = await _commercialService.GetReturnsAsync();
        return View(items);
    }

    [Authorize(Policy = "COMERCIAL.FACTURA_VENTA.CREAR")]
    public async Task<IActionResult> Create(Guid? invoiceId)
    {
        var vm = await _commercialService.GetReturnFormContextAsync(invoiceId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.FACTURA_VENTA.CREAR")]
    public async Task<IActionResult> Create(SalesReturnFormViewModel vm)
    {
        var salesReturn = vm.Return;
        ModelState.Remove("Return.Factura");

        if (ModelState.IsValid)
        {
            var result = await _commercialService.CreateReturnAsync(salesReturn);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        return View(vm);
    }
}
