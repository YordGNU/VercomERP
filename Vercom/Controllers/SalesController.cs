using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class SalesController : Controller
{
   private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
    private readonly ISalesService _salesService;

    public SalesController(AppDbContext context, ISalesService salesService)
    {
        _context = context;
        _salesService = salesService;
    }

    public async Task<IActionResult> Index()
    {
        var entidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        var invoices = await _context.FacturaVenta
            .Include(f => f.Cliente)
            .Where(f => f.EntidadId == entidadId)
            .OrderByDescending(f => f.Fecha)
            .ToListAsync();
        return View(invoices);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var entidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.EntidadId == entidadId && c.Activo), "Id", "NombreRazonSocial");
        ViewData["AlmacenId"] = new SelectList(_context.Almacens.Where(a => a.EntidadId == entidadId && a.Activo), "Id", "Nombre");
        ViewData["ContratoId"] = new SelectList(_context.ContratoEconomicos.Where(c => c.EntidadId == entidadId && c.Estado == "VIGENTE"), "Id", "NumeroContrato");

        return View(new FacturaVentum { Serie = "A", Fecha = DateTimeOffset.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FacturaVentum invoice)
    {
        if (ModelState.IsValid)
        {
            invoice.EntidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
            invoice.SucursalId = Guid.Parse(User.FindFirst("SucursalId")?.Value ?? Guid.Empty.ToString());
            invoice.CreadoPor = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            invoice.CanalVenta = "ERP";

            var result = await _salesService.CreateInvoiceAsync(invoice);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }

        var entidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.EntidadId == entidadId), "Id", "NombreRazonSocial", invoice.ClienteId);
        ViewData["AlmacenId"] = new SelectList(_context.Almacens.Where(a => a.EntidadId == entidadId), "Id", "Nombre", invoice.AlmacenId);
        return View(invoice);
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(Guid id, string reason)
    {
        var result = await _salesService.CancelInvoiceAsync(id, reason);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
