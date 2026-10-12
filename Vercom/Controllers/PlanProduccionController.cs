using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [Authorize(Policy = "PRODUCCION.FICHA.VER")]
    public async Task<IActionResult> Details(Guid id)
    {
        var plan = await _productionService.GetProductionPlanByIdAsync(id);
        if (plan == null) return NotFound();
        return View(plan);
    }

    [Authorize(Policy = "PRODUCCION.FICHA.CREAR")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var plan = await _productionService.GetProductionPlanByIdAsync(id);
        if (plan == null) return NotFound();

        var vm = await _productionService.GetPlanFormContextAsync(plan);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PRODUCCION.FICHA.CREAR")]
    public async Task<IActionResult> Edit(Guid id, PlanProduccionFormViewModel vm)
    {
        var plan = vm.Plan;
        plan.Id = id;

        ModelState.Remove("Plan.Entidad");
        ModelState.Remove("Plan.Presupuesto");

        if (ModelState.IsValid)
        {
            var result = await _productionService.UpdatePlanAsync(plan);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Message);
        }

        var existing = await _productionService.GetProductionPlanByIdAsync(id);
        var refreshedVm = await _productionService.GetPlanFormContextAsync(existing ?? vm.Plan);
        return View(refreshedVm);
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
