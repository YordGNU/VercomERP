using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.Security;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class SalesController : Controller
{
    private readonly ISalesService _salesService;
    private readonly IEntidadProvider _entidadProvider;

    public SalesController(ISalesService salesService, IEntidadProvider entidadProvider)
    {
        _salesService = salesService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "COMERCIAL.FACTURA_VENTA.VER")]
    public async Task<IActionResult> Index()
    {
        var invoices = await _salesService.GetInvoicesAsync();
        return View(invoices);
    }

    [Authorize(Policy = "COMERCIAL.FACTURA_VENTA.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var invoice = await _salesService.GetInvoiceByIdAsync(id.Value);
        if (invoice == null) return NotFound();

        return View(invoice);
    }

    [HttpGet]
    [Authorize(Policy = "COMERCIAL.FACTURA_VENTA.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _salesService.GetSalesCreateContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.FACTURA_VENTA.CREAR")]
    public async Task<IActionResult> Create(SalesCreateViewModel vm)
    {
        var invoice = vm.Invoice;

        // Limpiar validaciones
        ModelState.Remove("Invoice.Almacen");
        ModelState.Remove("Invoice.Cliente");
        ModelState.Remove("Invoice.Entidad");
        ModelState.Remove("Invoice.Sucursal");
        ModelState.Remove("Invoice.Contrato");
        ModelState.Remove("Invoice.NumeroFactura");
        ModelState.Remove("Invoice.EntidadId");
        ModelState.Remove("Invoice.CreadoEn");

        if (ModelState.IsValid)
        {
            invoice.EntidadId = _entidadProvider.CurrentEntidadId;
            invoice.CreadoPor = _entidadProvider.CurrentUsuarioId;
            invoice.CanalVenta = "ERP";
            invoice.Moneda = "CUP";
            invoice.Estado = "EMITIDA";
            invoice.Fecha = DateTimeOffset.Now;

            var result = await _salesService.CreateInvoiceAsync(invoice);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = result.Invoice?.Id });
            }
            ModelState.AddModelError("", result.Message);
        }
        else
        {
            var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            ModelState.AddModelError("", $"Verifique los datos: {errors}");
        }

        var contextVm = await _salesService.GetSalesCreateContextAsync(invoice);
        return View(contextVm);
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
