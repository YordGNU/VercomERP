using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.Security;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class PurchaseController : Controller
{
    private readonly IPurchaseService _purchaseService;
    private readonly IEntidadProvider _entidadProvider;

    public PurchaseController(IPurchaseService purchaseService, IEntidadProvider entidadProvider)
    {
        _purchaseService = purchaseService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "COMERCIAL.ORDEN_COMPRA.VER")]
    public async Task<IActionResult> Index()
    {
        var orders = await _purchaseService.GetPurchaseOrdersAsync();
        return View(orders);
    }

    [Authorize(Policy = "COMERCIAL.ORDEN_COMPRA.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var order = await _purchaseService.GetPurchaseOrderByIdAsync(id.Value);
        if (order == null) return NotFound();

        return View(order);
    }

    [HttpGet]
    [Authorize(Policy = "COMERCIAL.ORDEN_COMPRA.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _purchaseService.GetPurchaseOrderCreateContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.ORDEN_COMPRA.CREAR")]
    public async Task<IActionResult> Create(PurchaseOrderViewModel vm)
    {
        var order = vm.Order;

        ModelState.Remove("Order.Entidad");
        ModelState.Remove("Order.Proveedor");
        ModelState.Remove("Order.EntidadId");
        ModelState.Remove("Order.NumeroOrden");

        if (ModelState.IsValid)
        {
            order.EntidadId = _entidadProvider.CurrentEntidadId;
            order.CreadoPor = _entidadProvider.CurrentUsuarioId;

            var result = await _purchaseService.CreatePurchaseOrderAsync(order);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = result.Order?.Id });
            }
            ModelState.AddModelError("", result.Message);
        }

        var contextVm = await _purchaseService.GetPurchaseOrderCreateContextAsync(order);
        return View(contextVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.ORDEN_COMPRA.APROBAR")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _purchaseService.ApprovePurchaseOrderAsync(id, _entidadProvider.CurrentUsuarioId);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    [Authorize(Policy = "COMERCIAL.ORDEN_COMPRA.CREAR")]
    public async Task<IActionResult> Receive(Guid id)
    {
        var order = await _purchaseService.GetPurchaseOrderByIdAsync(id);
        if (order == null) return NotFound();
        if (order.Estado != "APROBADA") return BadRequest("La orden no está aprobada para recepción.");

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.ORDEN_COMPRA.CREAR")]
    public async Task<IActionResult> Receive(Guid id, Guid almacenId, string receiptNumber)
    {
        var result = await _purchaseService.ReceivePurchaseAsync(id, almacenId, _entidadProvider.CurrentUsuarioId, receiptNumber);

        if (result.Succeeded)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Receive), new { id });
    }
}
