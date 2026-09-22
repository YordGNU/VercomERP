using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class AlmacenController : Controller
{
    private readonly IInventoryService _inventoryService;

    public AlmacenController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [Authorize(Policy = "INVENTARIO.ALMACEN.VER")]
    public async Task<IActionResult> Index()
    {
        var almacenes = await _inventoryService.GetWarehousesAsync();
        return View(almacenes);
    }

    [Authorize(Policy = "INVENTARIO.ALMACEN.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();
        var almacen = await _inventoryService.GetWarehouseByIdAsync(id.Value);
        if (almacen == null) return NotFound();
        return View(almacen);
    }

    [Authorize(Policy = "INVENTARIO.ALMACEN.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _inventoryService.GetWarehouseFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.ALMACEN.CREAR")]
    public async Task<IActionResult> Create(AlmacenFormViewModel vm)
    {
        var almacen = vm.Almacen;
        ModelState.Remove("Almacen.Entidad");
        ModelState.Remove("Almacen.Sucursal");

        if (ModelState.IsValid)
        {
            var result = await _inventoryService.CreateWarehouseAsync(almacen);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }

    [Authorize(Policy = "INVENTARIO.ALMACEN.CREAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var almacen = await _inventoryService.GetWarehouseByIdAsync(id.Value);
        if (almacen == null) return NotFound();

        var vm = await _inventoryService.GetWarehouseFormContextAsync(almacen);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.ALMACEN.CREAR")]
    public async Task<IActionResult> Edit(Guid id, AlmacenFormViewModel vm)
    {
        var almacen = vm.Almacen;
        if (id != almacen.Id) return NotFound();

        ModelState.Remove("Almacen.Entidad");
        ModelState.Remove("Almacen.Sucursal");

        if (ModelState.IsValid)
        {
            var result = await _inventoryService.UpdateWarehouseAsync(almacen);
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
