using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class MantenimientoProgramadoController : Controller
{
    private readonly IProductionService _productionService;

    public MantenimientoProgramadoController(IProductionService productionService)
    {
        _productionService = productionService;
    }

    [Authorize(Policy = "PRODUCCION.MANTENIMIENTO.VER")]
    public async Task<IActionResult> Index()
    {
        var items = await _productionService.GetMaintenancesAsync();
        return View(items);
    }

    [Authorize(Policy = "PRODUCCION.MANTENIMIENTO.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _productionService.GetMaintenanceFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PRODUCCION.MANTENIMIENTO.CREAR")]
    public async Task<IActionResult> Create(MantenimientoFormViewModel vm)
    {
        var maintenance = vm.Mantenimiento;
        ModelState.Remove("Mantenimiento.Equipo");
        ModelState.Remove("Mantenimiento.Responsable");

        if (ModelState.IsValid)
        {
            var result = await _productionService.CreateMaintenanceAsync(maintenance);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _productionService.GetMaintenanceFormContextAsync(maintenance);
        return View(contextVm);
    }
}
