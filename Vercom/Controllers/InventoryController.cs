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
    private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

    public InventoryController(AppDbContext context, IInventoryService inventoryService)
    {
        _context = context;
        _inventoryService = inventoryService;
    }

    [Authorize(Policy = "INVENTARIO.EXISTENCIA.VER")]
    public async Task<IActionResult> Index()
    {
        var stocks = await _context.Existencia
            .Include(e => e.Producto)
            .Include(e => e.Almacen)
            .Where(e => e.Almacen.EntidadId == CurrentEntidadId)
            .OrderBy(e => e.Almacen.Nombre)
            .ToListAsync();
        return View(stocks);
    }

    [Authorize(Policy = "INVENTARIO.EXISTENCIA.VER")]
    public async Task<IActionResult> LowStock()
    {
        var alerts = await _inventoryService.GetLowStockAlertsAsync(CurrentEntidadId);
        return View(alerts);
    }

    [Authorize(Policy = "INVENTARIO.MOVIMIENTO.VER")]
    public async Task<IActionResult> Movements()
    {
        var movements = await _context.MovimientoInventarios
            .Include(m => m.TipoMovimiento)
            .Include(m => m.AlmacenOrigen)
            .Include(m => m.AlmacenDestino)
            .Where(m => m.EntidadId == CurrentEntidadId)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();
        return View(movements);
    }

    [Authorize(Policy = "INVENTARIO.MOVIMIENTO.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var movement = await _context.MovimientoInventarios
            .Include(m => m.TipoMovimiento)
            .Include(m => m.AlmacenOrigen)
            .Include(m => m.AlmacenDestino)
            .Include(m => m.MovimientoInventarioDetalles).ThenInclude(d => d.Producto).ThenInclude(p => p.UnidadMedida)
            .Include(m => m.Asiento)
            .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

        if (movement == null) return NotFound();

        return View(movement);
    }

    [HttpGet]
    [Authorize(Policy = "INVENTARIO.MOVIMIENTO.CREAR")]
    public IActionResult CreateMovement()
    {
        ViewData["TipoMovimientoId"] = new SelectList(_context.TipoMovimientos.OrderBy(t => t.Nombre), "Id", "Nombre");
        ViewData["Almacenes"] = new SelectList(_context.Almacens.Where(a => a.EntidadId == CurrentEntidadId && a.Activo), "Id", "Nombre");

        ViewBag.Productos = _context.Productos
            .Where(p => p.EntidadId == CurrentEntidadId && p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new { p.Id, Display = p.Codigo + " - " + p.Nombre })
            .ToList();

        return View(new MovimientoInventario { Fecha = DateTimeOffset.Now, Canal = "ERP" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.MOVIMIENTO.CREAR")]
    public async Task<IActionResult> CreateMovement(MovimientoInventario movement)
    {
        if (ModelState.IsValid)
        {
            movement.EntidadId = CurrentEntidadId;
            movement.CreadoPor = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            movement.Canal = "ERP";

            var result = await _inventoryService.ProcessMovementAsync(movement);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = result.Movement?.Id });
            }
            ModelState.AddModelError("", result.Message);
        }

        ViewData["TipoMovimientoId"] = new SelectList(_context.TipoMovimientos, "Id", "Nombre", movement.TipoMovimientoId);
        ViewData["Almacenes"] = new SelectList(_context.Almacens.Where(a => a.EntidadId == CurrentEntidadId), "Id", "Nombre");
        return View(movement);
    }
}
