using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class TopePrecioMfpController : Controller
{
    private readonly ICommercialService _commercialService;

    public TopePrecioMfpController(ICommercialService commercialService)
    {
        _commercialService = commercialService;
    }

    [Authorize(Policy = "COMERCIAL.CLIENTE.VER")]
    public async Task<IActionResult> Index()
    {
        var items = await _commercialService.GetPriceLimitsAsync();
        return View(items);
    }

    [Authorize(Policy = "COMERCIAL.CLIENTE.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _commercialService.GetPriceLimitFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.CLIENTE.CREAR")]
    public async Task<IActionResult> Create(PriceLimitFormViewModel vm)
    {
        var limit = vm.Limit;
        ModelState.Remove("Limit.Producto");
        ModelState.Remove("Limit.Familia");

        if (ModelState.IsValid)
        {
            var result = await _commercialService.CreatePriceLimitAsync(limit);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        return View(vm);
    }
}
