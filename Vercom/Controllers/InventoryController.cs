using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly IWarehouseService _warehouseService;

    public InventoryController(AppDbContext context, IInventoryService inventoryService, IWarehouseService warehouseService)
    {
        _context = context;
        _inventoryService = inventoryService;
        _warehouseService = warehouseService;
    }

    public async Task<IActionResult> Index()
    {
        var entidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        var stocks = await _context.Existencia
            .Include(e => e.Producto)
            .Include(e => e.Almacen)
            .Where(e => e.Almacen.EntidadId == entidadId)
            .ToListAsync();
        return View(stocks);
    }

    public async Task<IActionResult> LowStock()
    {
        var entidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        var alerts = await _inventoryService.GetLowStockAlertsAsync(entidadId);
        return View(alerts);
    }

    [HttpGet]
    public IActionResult CreateMovement()
    {
        ViewData["TipoMovimientoId"] = new SelectList(_context.TipoMovimientos, "Id", "Nombre");
        ViewData["AlmacenId"] = new SelectList(_context.Almacens, "Id", "Nombre");
        ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Nombre");
        return View(new MovimientoInventario { Fecha = DateTimeOffset.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMovement(MovimientoInventario movement)
    {
        if (ModelState.IsValid)
        {
            movement.EntidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
            movement.CreadoPor = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            var result = await _inventoryService.ProcessMovementAsync(movement);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }

        ViewData["TipoMovimientoId"] = new SelectList(_context.TipoMovimientos, "Id", "Nombre", movement.TipoMovimientoId);
        return View(movement);
    }
}
