using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class FamiliaProductoController : Controller
{
    private readonly IInventoryService _inventoryService;

    public FamiliaProductoController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [Authorize(Policy = "INVENTARIO.FAMILIA.VER")]
    public async Task<IActionResult> Index()
    {
        var familias = await _inventoryService.GetFamiliesAsync();
        return View(familias);
    }

    [Authorize(Policy = "INVENTARIO.FAMILIA.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _inventoryService.GetFamilyFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.FAMILIA.CREAR")]
    public async Task<IActionResult> Create(FamiliaFormViewModel vm)
    {
        var familia = vm.Familia;
        ModelState.Remove("Familia.Entidad");
        ModelState.Remove("Familia.FamiliaPadre");

        if (ModelState.IsValid)
        {
            var result = await _inventoryService.CreateFamilyAsync(familia);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _inventoryService.GetFamilyFormContextAsync(familia);
        return View(contextVm);
    }

    [Authorize(Policy = "INVENTARIO.FAMILIA.CREAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var familia = await _inventoryService.GetFamilyByIdAsync(id.Value);
        if (familia == null) return NotFound();

        var vm = await _inventoryService.GetFamilyFormContextAsync(familia);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.FAMILIA.CREAR")]
    public async Task<IActionResult> Edit(Guid id, FamiliaFormViewModel vm)
    {
        var familia = vm.Familia;
        if (id != familia.Id) return NotFound();

        ModelState.Remove("Familia.Entidad");
        ModelState.Remove("Familia.FamiliaPadre");

        if (ModelState.IsValid)
        {
            var result = await _inventoryService.UpdateFamilyAsync(familia);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _inventoryService.GetFamilyFormContextAsync(familia);
        return View(contextVm);
    }
}
