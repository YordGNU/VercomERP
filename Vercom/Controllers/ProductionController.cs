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
   private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
    private readonly IProductionService _productionService;

    public ProductionController(AppDbContext context, IProductionService productionService)
    {
        _context = context;
        _productionService = productionService;
    }

    public async Task<IActionResult> Index()
    {
        var entidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        var orders = await _context.OrdenProduccions
            .Include(o => o.FichaCosto)
            .Where(o => o.EntidadId == entidadId)
            .OrderByDescending(o => o.CreadoEn)
            .ToListAsync();
        return View(orders);
    }

    [HttpGet]
    public IActionResult CreateOrder()
    {
        var entidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        ViewData["ProductoTerminadoId"] = new SelectList(_context.Productos.Where(p => p.Tipo == "ELABORADO" || p.Tipo == "TERMINADO"), "Id", "Nombre");
        ViewData["AlmacenId"] = new SelectList(_context.Almacens.Where(a => a.EntidadId == entidadId), "Id", "Nombre");
        return View(new OrdenProduccion());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder(OrdenProduccion order)
    {
        if (ModelState.IsValid)
        {
            order.EntidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
            order.CreadoPor = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            var result = await _productionService.CreateProductionOrderAsync(order);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }
        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> Start(Guid id)
    {
        var result = await _productionService.StartProductionAsync(id);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Deviations()
    {
        var entidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        var deviations = await _context.AnalisisDesviacions
            .Include(a => a.OrdenProduccion)
            .Where(a => a.OrdenProduccion.EntidadId == entidadId)
            .OrderByDescending(a => a.AnalizadoEn)
            .ToListAsync();
        return View(deviations);
    }
}
