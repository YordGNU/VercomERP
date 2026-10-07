using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Security;
using Vercom.Services;

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
        ViewBag.Productos = await _inventoryService.GetCatalogAsync();
        return View(count);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.CONTEO.CREAR")]
    public async Task<IActionResult> Start(Guid almacenId)
    {
        var result = await _warehouseService.StartPhysicalCountAsync(almacenId, _entidadProvider.CurrentUsuarioId);
        if (result.Succeeded)
        {
            return Json(new { success = true, message = "Conteo iniciado correctamente.", redirectUrl = Url.Action(nameof(Details), new { id = result.Count?.Id }) });
        }

        return Json(new { success = false, message = result.Message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.CONTEO.CREAR")]
    public async Task<IActionResult> SubmitDetail(Guid conteoId, Guid productoId, decimal cantidadFisica, string? justificacion)
    {
        var result = await _warehouseService.SubmitCountDetailAsync(conteoId, productoId, cantidadFisica, justificacion);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Details), new { id = conteoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.CONTEO.CERRAR")]
    public async Task<IActionResult> Close(Guid id)
    {
        var result = await _warehouseService.CloseAndAdjustCountAsync(id, _entidadProvider.CurrentUsuarioId);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }
}
