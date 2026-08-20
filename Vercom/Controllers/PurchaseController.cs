using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class PurchaseController : Controller
{
   private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
    private readonly IPurchaseService _purchaseService;

    public PurchaseController(AppDbContext context, IPurchaseService purchaseService)
    {
        _context = context;
        _purchaseService = purchaseService;
    }

    public async Task<IActionResult> Index()
    {
        var entidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        var orders = await _context.OrdenCompras
            .Include(o => o.Proveedor)
            .Where(o => o.EntidadId == entidadId)
            .OrderByDescending(o => o.Fecha)
            .ToListAsync();
        return View(orders);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var entidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        ViewData["ProveedorId"] = new SelectList(_context.Proveedors.Where(p => p.EntidadId == entidadId && p.Activo), "Id", "RazonSocial");
        ViewData["AlmacenDestinoId"] = new SelectList(_context.Almacens.Where(a => a.EntidadId == entidadId), "Id", "Nombre");
        return View(new OrdenCompra { Fecha = DateOnly.FromDateTime(DateTime.Now) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrdenCompra order)
    {
        if (ModelState.IsValid)
        {
            order.EntidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
            order.CreadoPor = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            var result = await _purchaseService.CreatePurchaseOrderAsync(order);
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
    public async Task<IActionResult> Approve(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var result = await _purchaseService.ApprovePurchaseOrderAsync(id, userId);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Receive(Guid id, Guid almacenId, string receiptNumber)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var result = await _purchaseService.ReceivePurchaseAsync(id, almacenId, userId, receiptNumber);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
