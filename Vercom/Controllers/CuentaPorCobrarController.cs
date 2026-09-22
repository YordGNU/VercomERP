using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Security;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class CuentaPorCobrarController : Controller
{
    private readonly IReceivablesPayablesService _carteraService;
    private readonly IEntidadProvider _entidadProvider;

    public CuentaPorCobrarController(IReceivablesPayablesService carteraService, IEntidadProvider entidadProvider)
    {
        _carteraService = carteraService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "CONTABILIDAD.CXC.VER")]
    public async Task<IActionResult> Index(string? search, string? tipo, string? estado, bool vencidas = false, int page = 1)
    {
        var vm = await _carteraService.GetCxCIndexAsync(_entidadProvider.CurrentEntidadId, search, tipo, estado, vencidas, page);
        return View(vm);
    }

    [Authorize(Policy = "CONTABILIDAD.CXC.VER")]
    public async Task<IActionResult> Details(Guid id)
    {
        var item = await _carteraService.GetCxCByIdAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [Authorize(Policy = "CONTABILIDAD.CXC.VER")]
    public async Task<IActionResult> Create()
    {
        var vm = await _carteraService.GetCxCFormContextAsync(_entidadProvider.CurrentEntidadId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CXC.VER")]
    public async Task<IActionResult> Create(CxCFormViewModel vm)
    {
        if (vm.Item.ClienteId == Guid.Empty) ModelState.AddModelError("Item.ClienteId", "Seleccione el cliente.");
        if (string.IsNullOrWhiteSpace(vm.Item.DocumentoOrigenTipo)) ModelState.AddModelError("Item.DocumentoOrigenTipo", "Indique el tipo de documento.");
        if (vm.Item.MontoOriginal <= 0) ModelState.AddModelError("Item.MontoOriginal", "El importe debe ser mayor que cero.");
        if (vm.Item.FechaVencimiento < vm.Item.FechaEmision) ModelState.AddModelError("Item.FechaVencimiento", "El vencimiento no puede ser anterior a la emisión.");

        if (!ModelState.IsValid)
        {
            var reload = await _carteraService.GetCxCFormContextAsync(_entidadProvider.CurrentEntidadId, vm.Item);
            return View(reload);
        }

        var result = await _carteraService.CreateCxCAsync(vm.Item, _entidadProvider.CurrentEntidadId);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Message);
            var reload = await _carteraService.GetCxCFormContextAsync(_entidadProvider.CurrentEntidadId, vm.Item);
            return View(reload);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Details), new { id = vm.Item.Id });
    }

    [Authorize(Policy = "CONTABILIDAD.CXC.VER")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var item = await _carteraService.GetCxCByIdAsync(id);
        if (item == null) return NotFound();
        var vm = await _carteraService.GetCxCFormContextAsync(_entidadProvider.CurrentEntidadId, item);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CXC.VER")]
    public async Task<IActionResult> Edit(Guid id, CxCFormViewModel vm)
    {
        if (vm.Item.Id != id) return BadRequest();
        if (vm.Item.ClienteId == Guid.Empty) ModelState.AddModelError("Item.ClienteId", "Seleccione el cliente.");
        if (vm.Item.MontoOriginal <= 0) ModelState.AddModelError("Item.MontoOriginal", "El importe debe ser mayor que cero.");

        if (!ModelState.IsValid)
        {
            var reload = await _carteraService.GetCxCFormContextAsync(_entidadProvider.CurrentEntidadId, vm.Item);
            return View(reload);
        }

        var result = await _carteraService.UpdateCxCAsync(vm.Item, _entidadProvider.CurrentEntidadId);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Message);
            var reload = await _carteraService.GetCxCFormContextAsync(_entidadProvider.CurrentEntidadId, vm.Item);
            return View(reload);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.PAGO.CREAR")]
    public async Task<IActionResult> RecordCollection(PaymentRecordViewModel vm)
    {
        if (ModelState.IsValid && vm.Amount > 0)
        {
            var result = await _carteraService.RecordCollectionAsync(vm.ItemId, vm.Amount, vm.PaymentMethod, vm.Reference, _entidadProvider.CurrentUsuarioId);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Index), new { aplicar = vm.ItemId });
        }

        TempData["Error"] = "Los datos del cobro no son válidos.";
        return RedirectToAction(nameof(Index), vm.ItemId == Guid.Empty ? null : new { aplicar = vm.ItemId });
    }
}