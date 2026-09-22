using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class EquipoController : Controller
{
    private readonly IProductionService _productionService;

    public EquipoController(IProductionService productionService)
    {
        _productionService = productionService;
    }

    [Authorize(Policy = "PRODUCCION.FICHA.VER")]
    public async Task<IActionResult> Index()
    {
        var items = await _productionService.GetEquipmentsAsync();
        return View(items);
    }

    [Authorize(Policy = "PRODUCCION.FICHA.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _productionService.GetEquipmentFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PRODUCCION.FICHA.CREAR")]
    public async Task<IActionResult> Create(EquipoFormViewModel vm)
    {
        var equipment = vm.Equipo;
        ModelState.Remove("Equipo.Entidad");
        ModelState.Remove("Equipo.Sucursal");

        if (ModelState.IsValid)
        {
            var result = await _productionService.CreateEquipmentAsync(equipment);
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
