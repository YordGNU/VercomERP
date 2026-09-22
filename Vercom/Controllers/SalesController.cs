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

        ViewBag.TaxRate = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "TASA_IMP_VENTAS");
        if (ViewBag.TaxRate == 0) ViewBag.TaxRate = 10.0m; // Default 10%

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
        ModelState.Remove("Invoice.NumeroFactura");
        ModelState.Remove("Invoice.EntidadId");
        ModelState.Remove("Invoice.CreadoEn");

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
