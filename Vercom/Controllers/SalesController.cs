using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Security;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class SalesController : Controller
{
    private readonly ISalesService _salesService;
    private readonly IParametroSistemaService _paramService;
    private readonly IEntidadProvider _entidadProvider;
    private readonly ICommercialService _commercialService;

    public SalesController(ISalesService salesService, IParametroSistemaService paramService,
        IEntidadProvider entidadProvider, ICommercialService commercialService)
    {
        _salesService = salesService;
        _paramService = paramService;
        _entidadProvider = entidadProvider;
        _commercialService = commercialService;
    }

    [Authorize(Policy = "COMERCIAL.FACTURA_VENTA.VER")]
    public async Task<IActionResult> Index(string? search, string? status, string? channel)
    {
        var invoices = await _salesService.GetInvoicesAsync(search, status, channel);
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentStatus = status;
        ViewBag.CurrentChannel = channel;
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
        var entidadId = _entidadProvider.CurrentEntidadId;
        var vm = await _salesService.GetSalesCreateContextAsync();

        var tasaImpuesto = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "TAX_VENTA");
        ViewBag.TaxRate = tasaImpuesto * 100m;

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> CheckCreditLimit(Guid clienteId, decimal amount)
    {
        var result = await _commercialService.ValidateCreditLimitAsync(clienteId, amount);
        return Json(new { succeeded = result.Succeeded, message = result.Message });
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
        ModelState.Remove("Invoice.Produtos");
        ModelState.Remove("Invoice.FacturaVenta");
        ModelState.Remove("Invoice.FormaPagoVentaEntity");
        ModelState.Remove("Invoice.NumeroFactura");
        ModelState.Remove("Invoice.EntidadId");
        ModelState.Remove("Invoice.SucursalId");
        ModelState.Remove("Invoice.CreadoPor");
        ModelState.Remove("Invoice.CreadoEn");
        ModelState.Remove("Invoice.Fecha");
        ModelState.Remove("Invoice.CanalVenta");
        ModelState.Remove("Invoice.Moneda");
        ModelState.Remove("Invoice.Estado");
        ModelState.Remove("Invoice.Total");
        ModelState.Remove("Invoice.Subtotal");
        ModelState.Remove("Invoice.ImpuestoVentasTotal");
        ModelState.Remove("Invoice.AsientoId");
        ModelState.Remove("Invoice.CuentaPorCobrarId");
        ModelState.Remove("Invoice.DispositivoPosId");
        ModelState.Remove("Invoice.SesionCajaPosId");
        ModelState.Remove("Invoice.MotivoAnulacion");
        ModelState.Remove("Invoice.Factura");
        ModelState.Remove("Invoice.FacturaVentum");
        ModelState.Remove("Invoice.FacturaVentaDetalle");

        var serverManagedSuffixes = new[] { ".Producto", ".Factura", ".Entidad", ".Sucursal", ".Cliente", ".Almacen", ".Contrato", ".FormaPago" };
        var serverManagedKeys = ModelState.Keys
            .Where(k => k != null && serverManagedSuffixes.Any(s => k.EndsWith(s, StringComparison.Ordinal)))
            .ToList();
        foreach (var key in serverManagedKeys) ModelState.Remove(key);

        if (ModelState.IsValid)
        {
            invoice.EntidadId = _entidadProvider.CurrentEntidadId;
            invoice.SucursalId = _entidadProvider.CurrentSucursalId ?? invoice.SucursalId;
            invoice.CreadoPor = _entidadProvider.CurrentUsuarioId;
            invoice.CanalVenta = "ERP";
            invoice.Moneda = "CUP";
            invoice.Estado = "EMITIDA";
            invoice.Fecha = DateTimeOffset.Now;

            var result = await _salesService.CreateInvoiceAsync(invoice);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Details), new { id = result.Invoice?.Id }) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Verifique los datos de facturación.", errors = errorList });
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
