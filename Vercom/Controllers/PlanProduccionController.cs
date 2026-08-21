using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class PlanProduccionController : Controller
{
    private readonly IProductionService _productionService;

    public PlanProduccionController(IProductionService productionService)
    {
        _productionService = productionService;
    }

    [Authorize(Policy = "PRODUCCION.FICHA.VER")]
    public async Task<IActionResult> Index()
    {
        var items = await _productionService.GetProductionPlansAsync();
        return View(items);
    }

    [Authorize(Policy = "PRODUCCION.FICHA.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _productionService.GetPlanFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PRODUCCION.FICHA.CREAR")]
    public async Task<IActionResult> Create(PlanProduccionFormViewModel vm)
    {
        var plan = vm.Plan;
        ModelState.Remove("Plan.Entidad");
        ModelState.Remove("Plan.Presupuesto");

        if (ModelState.IsValid)
        {
            var result = await _productionService.CreatePlanAsync(plan);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _productionService.GetPlanFormContextAsync(plan);
        return View(contextVm);
    }
}
