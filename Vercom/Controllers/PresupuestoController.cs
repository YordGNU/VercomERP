using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class PresupuestoController : Controller
{
    private readonly IProductionService _productionService;

    public PresupuestoController(IProductionService productionService)
    {
        _productionService = productionService;
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.VER")]
    public async Task<IActionResult> Index()
    {
        var items = await _productionService.GetBudgetsAsync();
        return View(items);
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _productionService.GetBudgetFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CUENTA.CREAR")]
    public async Task<IActionResult> Create(PresupuestoFormViewModel vm)
    {
        var budget = vm.Presupuesto;
        ModelState.Remove("Presupuesto.Entidad");

        if (ModelState.IsValid)
        {
            var result = await _productionService.CreateBudgetAsync(budget);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        return View(vm);
    }
}
