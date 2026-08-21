using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.Security;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly IInventoryService _inventoryService;
    private readonly IEntidadProvider _entidadProvider;

    public InventoryController(IInventoryService inventoryService, IEntidadProvider entidadProvider)
    {
        _inventoryService = inventoryService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "INVENTARIO.EXISTENCIA.VER")]
    public async Task<IActionResult> Index()
    {
        var stocks = await _inventoryService.GetStocksAsync();
        return View(stocks);
    }

    [Authorize(Policy = "INVENTARIO.EXISTENCIA.VER")]
    public async Task<IActionResult> LowStock()
    {
        var alerts = await _inventoryService.GetLowStockAlertsAsync(_entidadProvider.CurrentEntidadId);
        return View(alerts);
    }

    [Authorize(Policy = "INVENTARIO.MOVIMIENTO.VER")]
    public async Task<IActionResult> Movements()
    {
        var movements = await _inventoryService.GetMovementsAsync();
        return View(movements);
    }

    [Authorize(Policy = "INVENTARIO.MOVIMIENTO.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var movement = await _inventoryService.GetMovementByIdAsync(id.Value);
        if (movement == null) return NotFound();

        return View(movement);
    }

    [HttpGet]
    [Authorize(Policy = "INVENTARIO.MOVIMIENTO.CREAR")]
    public async Task<IActionResult> CreateMovement()
    {
        var vm = await _inventoryService.GetMovementCreateContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.MOVIMIENTO.CREAR")]
    public async Task<IActionResult> CreateMovement(InventoryMovementCreateViewModel vm)
    {
        var movement = vm.Movement;

        ModelState.Remove("Movement.Entidad");
        ModelState.Remove("Movement.TipoMovimiento");
        ModelState.Remove("Movement.EntidadId");

        if (ModelState.IsValid)
        {
            movement.EntidadId = _entidadProvider.CurrentEntidadId;
            movement.CreadoPor = _entidadProvider.CurrentUsuarioId;
            movement.Canal = "ERP";

            var result = await _inventoryService.ProcessMovementAsync(movement);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = result.Movement?.Id });
            }
            ModelState.AddModelError("", result.Message);
        }

        var contextVm = await _inventoryService.GetMovementCreateContextAsync(movement);
        return View(contextVm);
    }
}
