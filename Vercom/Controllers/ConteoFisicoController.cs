using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.Security;

namespace Vercom.Controllers;

[Authorize]
public class ConteoFisicoController : Controller
{
    private readonly IWarehouseService _warehouseService;
    private readonly IInventoryService _inventoryService;
    private readonly IEntidadProvider _entidadProvider;

    public ConteoFisicoController(IWarehouseService warehouseService, IInventoryService inventoryService, IEntidadProvider entidadProvider)
    {
        _warehouseService = warehouseService;
        _inventoryService = inventoryService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "INVENTARIO.CONTEO.VER")]
    public async Task<IActionResult> Index()
    {
        var counts = await _warehouseService.GetCountsAsync();
        var warehouses = await _inventoryService.GetWarehousesAsync();
        ViewBag.Almacenes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(warehouses.Where(a => a.Activo), "Id", "Nombre");
        return View(counts);
    }

    [Authorize(Policy = "INVENTARIO.CONTEO.VER")]
    public async Task<IActionResult> Details(Guid id)
    {
        var count = await _warehouseService.GetCountByIdAsync(id);
        if (count == null) return NotFound();
        return View(count);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.CONTEO.CREAR")]
    public async Task<IActionResult> Start(Guid almacenId)
    {
        var result = await _warehouseService.StartPhysicalCountAsync(almacenId, _entidadProvider.CurrentUsuarioId);
        if (result.Succeeded) return RedirectToAction(nameof(Details), new { id = result.Count?.Id });

        TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
