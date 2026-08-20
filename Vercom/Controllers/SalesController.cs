using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class SalesController : Controller
{
    private readonly AppDbContext _context;
    private readonly ISalesService _salesService;
    private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

    public SalesController(AppDbContext context, ISalesService salesService)
    {
        _context = context;
        _salesService = salesService;
    }

    [Authorize(Policy = "COMERCIAL.FACTURA_VENTA.VER")]
    public async Task<IActionResult> Index()
    {
        var invoices = await _context.FacturaVenta
            .Include(f => f.Cliente)
            .Where(f => f.EntidadId == CurrentEntidadId)
            .OrderByDescending(f => f.Fecha)
            .ToListAsync();
        return View(invoices);
    }

    [Authorize(Policy = "COMERCIAL.FACTURA_VENTA.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var invoice = await _context.FacturaVenta
            .Include(f => f.Cliente)
            .Include(f => f.Contrato)
            .Include(f => f.FacturaVentaDetalles).ThenInclude(d => d.Producto).ThenInclude(p => p.UnidadMedida)
            .Include(f => f.FormaPagoVenta)
            .Include(f => f.Asiento)
            .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

        if (invoice == null) return NotFound();

        return View(invoice);
    }

    [HttpGet]
    [Authorize(Policy = "COMERCIAL.FACTURA_VENTA.CREAR")]
    public IActionResult Create()
    {
        ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.EntidadId == CurrentEntidadId && c.Activo), "Id", "NombreRazonSocial");
        ViewData["AlmacenId"] = new SelectList(_context.Almacens.Where(a => a.EntidadId == CurrentEntidadId && a.Activo && a.EsPuntoVenta), "Id", "Nombre");
        ViewData["ContratoId"] = new SelectList(_context.ContratoEconomicos.Where(c => c.EntidadId == CurrentEntidadId && c.Estado == "VIGENTE" && c.TerceroTipo == "CLIENTE"), "Id", "NumeroContrato");

        ViewBag.Productos = _context.Productos
            .Where(p => p.EntidadId == CurrentEntidadId && p.Activo && (p.Tipo == "TERMINADO" || p.Tipo == "ELABORADO"))
            .Select(p => new { p.Id, p.Nombre, p.PrecioVentaActual, p.Codigo })
            .ToList();

        return View(new FacturaVentum {
            Serie = "A",
            Fecha = DateTimeOffset.Now,
            TipoVenta = "MINORISTA",
            CanalVenta = "ERP"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.FACTURA_VENTA.CREAR")]
    public async Task<IActionResult> Create(FacturaVentum invoice)
    {
        if (ModelState.IsValid)
        {
            invoice.EntidadId = CurrentEntidadId;
            invoice.CreadoPor = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            invoice.CanalVenta = "ERP";

            var result = await _salesService.CreateInvoiceAsync(invoice);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = result.Invoice?.Id });
            }
            ModelState.AddModelError("", result.Message);
        }

        // Si hay error, recargar ViewBags
        ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.EntidadId == CurrentEntidadId), "Id", "NombreRazonSocial", invoice.ClienteId);
        ViewData["AlmacenId"] = new SelectList(_context.Almacens.Where(a => a.EntidadId == CurrentEntidadId && a.EsPuntoVenta), "Id", "Nombre", invoice.AlmacenId);
        ViewData["ContratoId"] = new SelectList(_context.ContratoEconomicos.Where(c => c.EntidadId == CurrentEntidadId), "Id", "NumeroContrato", invoice.ContratoId);
        return View(invoice);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.FACTURA_VENTA.ANULAR")]
    public async Task<IActionResult> Cancel(Guid id, string reason)
    {
        var result = await _salesService.CancelInvoiceAsync(id, reason);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
