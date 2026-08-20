using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class ProductionController : Controller
{
    private readonly AppDbContext _context;
    private readonly IProductionService _productionService;
    private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

    public ProductionController(AppDbContext context, IProductionService productionService)
    {
        _context = context;
        _productionService = productionService;
    }

    [Authorize(Policy = "PRODUCCION.ORDEN.VER")]
    public async Task<IActionResult> Index()
    {
        var orders = await _context.OrdenProduccions
            .Include(o => o.ProductoTerminado)
            .Include(o => o.FichaCosto)
            .Where(o => o.EntidadId == CurrentEntidadId)
            .OrderByDescending(o => o.CreadoEn)
            .ToListAsync();
        return View(orders);
    }

    [Authorize(Policy = "PRODUCCION.ORDEN.VER")]
    public async Task<IActionResult> Details(Guid id)
    {
        var order = await _context.OrdenProduccions
            .Include(o => o.ProductoTerminado).ThenInclude(p => p.UnidadMedida)
            .Include(o => o.AlmacenInsumos)
            .Include(o => o.AlmacenProducto)
            .Include(o => o.OrdenProduccionConsumos).ThenInclude(c => c.ProductoInsumo).ThenInclude(p => p.UnidadMedida)
            .Include(o => o.FichaCosto)
            .Include(o => o.AsientoTerminado)
            .FirstOrDefaultAsync(o => o.Id == id && o.EntidadId == CurrentEntidadId);

        if (order == null) return NotFound();

        return View(order);
    }

    [HttpGet]
    [Authorize(Policy = "PRODUCCION.ORDEN.CREAR")]
    public IActionResult CreateOrder(Guid? productId)
    {
        ViewData["ProductoTerminadoId"] = new SelectList(_context.Productos
            .Where(p => p.EntidadId == CurrentEntidadId && (p.Tipo == "ELABORADO" || p.Tipo == "TERMINADO")), "Id", "Nombre", productId);

        ViewData["AlmacenId"] = new SelectList(_context.Almacens.Where(a => a.EntidadId == CurrentEntidadId && a.Activo), "Id", "Nombre");

        return View(new OrdenProduccion { FechaInicioPlan = DateOnly.FromDateTime(DateTime.Now) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PRODUCCION.ORDEN.CREAR")]
    public async Task<IActionResult> CreateOrder(OrdenProduccion order)
    {
        if (ModelState.IsValid)
        {
            order.EntidadId = CurrentEntidadId;
            order.CreadoPor = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            var result = await _productionService.CreateProductionOrderAsync(order);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }

        ViewData["ProductoTerminadoId"] = new SelectList(_context.Productos.Where(p => p.EntidadId == CurrentEntidadId), "Id", "Nombre", order.ProductoTerminadoId);
        ViewData["AlmacenId"] = new SelectList(_context.Almacens.Where(a => a.EntidadId == CurrentEntidadId), "Id", "Nombre");
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PRODUCCION.ORDEN.EJECUTAR")]
    public async Task<IActionResult> Start(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var result = await _productionService.StartProductionAndConsumeAsync(id, userId);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    [Authorize(Policy = "PRODUCCION.ORDEN.EJECUTAR")]
    public async Task<IActionResult> Finish(Guid id)
    {
        var order = await _context.OrdenProduccions
            .Include(o => o.ProductoTerminado)
            .Include(o => o.OrdenProduccionConsumos).ThenInclude(c => c.ProductoInsumo)
            .FirstOrDefaultAsync(o => o.Id == id && o.EntidadId == CurrentEntidadId);

        if (order == null) return NotFound();
        if (order.Estado != "EN_PROCESO") return BadRequest("La orden no está en proceso.");

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PRODUCCION.ORDEN.EJECUTAR")]
    public async Task<IActionResult> Finish(Guid id, decimal actualQuantity, List<OrdenProduccionConsumo> actualConsumptions)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var result = await _productionService.FinishProductionAsync(id, actualQuantity, actualConsumptions, userId);

        if (result.Succeeded)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Finish), new { id });
    }

    [Authorize(Policy = "PRODUCCION.ORDEN.VER")]
    public async Task<IActionResult> Deviations()
    {
        var deviations = await _context.AnalisisDesviacions
            .Include(a => a.OrdenProduccion).ThenInclude(o => o.ProductoTerminado)
            .Where(a => a.OrdenProduccion.EntidadId == CurrentEntidadId)
            .OrderByDescending(a => a.AnalizadoEn)
            .ToListAsync();
        return View(deviations);
    }
}
