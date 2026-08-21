using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.Security;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class ProductionController : Controller
{
    private readonly IProductionService _productionService;
    private readonly IEntidadProvider _entidadProvider;

    public ProductionController(IProductionService productionService, IEntidadProvider entidadProvider)
    {
        _productionService = productionService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "PRODUCCION.ORDEN.VER")]
    public async Task<IActionResult> Index()
    {
        var orders = await _productionService.GetOrdersAsync();
        return View(orders);
    }

    [Authorize(Policy = "PRODUCCION.ORDEN.VER")]
    public async Task<IActionResult> Details(Guid id)
    {
        var order = await _productionService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();
        return View(order);
    }

    [HttpGet]
    [Authorize(Policy = "PRODUCCION.ORDEN.CREAR")]
    public async Task<IActionResult> CreateOrder(Guid? productId)
    {
        var vm = await _productionService.GetProductionOrderCreateContextAsync();
        if (productId.HasValue) vm.Order.ProductoTerminadoId = productId.Value;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PRODUCCION.ORDEN.CREAR")]
    public async Task<IActionResult> CreateOrder(ProductionOrderViewModel vm)
    {
        var order = vm.Order;

        ModelState.Remove("Order.Entidad");
        ModelState.Remove("Order.ProductoTerminado");
        ModelState.Remove("Order.EntidadId");
        ModelState.Remove("Order.NumeroOrden");

        if (ModelState.IsValid)
        {
            order.EntidadId = _entidadProvider.CurrentEntidadId;
            order.CreadoPor = _entidadProvider.CurrentUsuarioId;

            var result = await _productionService.CreateProductionOrderAsync(order);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }

        var contextVm = await _productionService.GetProductionOrderCreateContextAsync(order);
        return View(contextVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PRODUCCION.ORDEN.EJECUTAR")]
    public async Task<IActionResult> Start(Guid id)
    {
        var result = await _productionService.StartProductionAndConsumeAsync(id, _entidadProvider.CurrentUsuarioId);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    [Authorize(Policy = "PRODUCCION.ORDEN.EJECUTAR")]
    public async Task<IActionResult> Finish(Guid id)
    {
        var order = await _productionService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();
        if (order.Estado != "EN_PROCESO") return BadRequest("La orden no está en proceso.");

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PRODUCCION.ORDEN.EJECUTAR")]
    public async Task<IActionResult> Finish(Guid id, decimal actualQuantity, List<OrdenProduccionConsumo> actualConsumptions)
    {
        var result = await _productionService.FinishProductionAsync(id, actualQuantity, actualConsumptions, _entidadProvider.CurrentUsuarioId);

        if (result.Succeeded)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Finish), new { id });
    }

    [Authorize(Policy = "PRODUCCION.FICHA.VER")]
    public async Task<IActionResult> Deviations()
    {
        var deviations = await _productionService.GetDeviationsAsync();
        return View(deviations);
    }
}
