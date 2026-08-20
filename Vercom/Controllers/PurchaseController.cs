using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class PurchaseController : Controller
{
    private readonly AppDbContext _context;
    private readonly IPurchaseService _purchaseService;
    private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

    public PurchaseController(AppDbContext context, IPurchaseService purchaseService)
    {
        _context = context;
        _purchaseService = purchaseService;
    }

    [Authorize(Policy = "COMERCIAL.ORDEN_COMPRA.VER")]
    public async Task<IActionResult> Index()
    {
        var orders = await _context.OrdenCompras
            .Include(o => o.Proveedor)
            .Where(o => o.EntidadId == CurrentEntidadId)
            .OrderByDescending(o => o.Fecha)
            .ToListAsync();
        return View(orders);
    }

    [Authorize(Policy = "COMERCIAL.ORDEN_COMPRA.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var order = await _context.OrdenCompras
            .Include(o => o.Proveedor)
            .Include(o => o.Contrato)
            .Include(o => o.AlmacenDestino)
            .Include(o => o.OrdenCompraDetalles).ThenInclude(d => d.Producto).ThenInclude(p => p.UnidadMedida)
            .Include(o => o.RecepcionCompras)
            .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

        if (order == null) return NotFound();

        return View(order);
    }

    [HttpGet]
    [Authorize(Policy = "COMERCIAL.ORDEN_COMPRA.CREAR")]
    public IActionResult Create()
    {
        ViewData["ProveedorId"] = new SelectList(_context.Proveedors.Where(p => p.EntidadId == CurrentEntidadId && p.Activo), "Id", "RazonSocial");
        ViewData["AlmacenDestinoId"] = new SelectList(_context.Almacens.Where(a => a.EntidadId == CurrentEntidadId && a.Activo), "Id", "Nombre");
        ViewData["ContratoId"] = new SelectList(_context.ContratoEconomicos.Where(c => c.EntidadId == CurrentEntidadId && c.Estado == "VIGENTE" && c.TerceroTipo == "PROVEEDOR"), "Id", "NumeroContrato");

        ViewBag.Productos = _context.Productos
            .Where(p => p.EntidadId == CurrentEntidadId && p.Activo)
            .Select(p => new { p.Id, p.Nombre, p.Codigo })
            .ToList();

        return View(new OrdenCompra { Fecha = DateOnly.FromDateTime(DateTime.Now) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.ORDEN_COMPRA.CREAR")]
    public async Task<IActionResult> Create(OrdenCompra order)
    {
        if (ModelState.IsValid)
        {
            order.EntidadId = CurrentEntidadId;
            order.CreadoPor = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            var result = await _purchaseService.CreatePurchaseOrderAsync(order);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = result.Order?.Id });
            }
            ModelState.AddModelError("", result.Message);
        }

        ViewData["ProveedorId"] = new SelectList(_context.Proveedors.Where(p => p.EntidadId == CurrentEntidadId), "Id", "RazonSocial", order.ProveedorId);
        ViewData["AlmacenDestinoId"] = new SelectList(_context.Almacens.Where(a => a.EntidadId == CurrentEntidadId), "Id", "Nombre", order.AlmacenDestinoId);
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.ORDEN_COMPRA.APROBAR")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var result = await _purchaseService.ApprovePurchaseOrderAsync(id, userId);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    [Authorize(Policy = "COMERCIAL.ORDEN_COMPRA.CREAR")] // Reutilizando para recepción
    public async Task<IActionResult> Receive(Guid id)
    {
        var order = await _context.OrdenCompras
            .Include(o => o.Proveedor)
            .Include(o => o.AlmacenDestino)
            .FirstOrDefaultAsync(o => o.Id == id && o.EntidadId == CurrentEntidadId);

        if (order == null) return NotFound();
        if (order.Estado != "APROBADA") return BadRequest("La orden no está aprobada para recepción.");

        ViewData["AlmacenId"] = new SelectList(_context.Almacens.Where(a => a.EntidadId == CurrentEntidadId && a.Activo), "Id", "Nombre", order.AlmacenDestinoId);
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.ORDEN_COMPRA.CREAR")]
    public async Task<IActionResult> Receive(Guid id, Guid almacenId, string receiptNumber)
    {
        if (string.IsNullOrEmpty(receiptNumber))
        {
            TempData["Error"] = "El número de informe de recepción es obligatorio.";
            return RedirectToAction(nameof(Receive), new { id });
        }

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var result = await _purchaseService.ReceivePurchaseAsync(id, almacenId, userId, receiptNumber);

        if (result.Succeeded)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Receive), new { id });
    }
}
