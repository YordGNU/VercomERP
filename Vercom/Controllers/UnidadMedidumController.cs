using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class UnidadMedidumController : Controller
{
    private readonly IInventoryService _inventoryService;

    public UnidadMedidumController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public async Task<IActionResult> Index()
    {
        var units = await _inventoryService.GetUnitsAsync();
        return View(units);
    }

    public async Task<IActionResult> Create()
    {
        var vm = await _inventoryService.GetUnitFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UnidadFormViewModel vm)
    {
        var unit = vm.Unidad;
        if (ModelState.IsValid)
        {
            var result = await _inventoryService.CreateUnitAsync(unit);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        return View(vm);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var unit = await _inventoryService.GetUnitByIdAsync(id.Value);
        if (unit == null) return NotFound();

        var vm = await _inventoryService.GetUnitFormContextAsync(unit);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UnidadFormViewModel vm)
    {
        var unit = vm.Unidad;
        if (id != unit.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var result = await _inventoryService.UpdateUnitAsync(unit);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        return View(vm);
    }
}
