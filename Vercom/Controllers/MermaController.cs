using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class MermaController : Controller
{
    private readonly IProductionService _productionService;

    public MermaController(IProductionService productionService)
    {
        _productionService = productionService;
    }

    [Authorize(Policy = "PRODUCCION.MERMA.VER")]
    public async Task<IActionResult> Index()
    {
        var items = await _productionService.GetWastesAsync();
        return View(items);
    }

    [Authorize(Policy = "PRODUCCION.MERMA.REGISTRAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _productionService.GetWasteFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PRODUCCION.MERMA.REGISTRAR")]
    public async Task<IActionResult> Create(MermaFormViewModel vm)
    {
        var waste = vm.Merma;
        ModelState.Remove("Merma.Producto");
        ModelState.Remove("Merma.OrdenProduccion");

        if (ModelState.IsValid)
        {
            var result = await _productionService.CreateWasteAsync(waste);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }
}
