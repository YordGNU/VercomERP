using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.Security;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class AsientoContableController : Controller
{
    private readonly IAccountingService _accountingService;
    private readonly IEntidadProvider _entidadProvider;

    public AsientoContableController(IAccountingService accountingService, IEntidadProvider entidadProvider)
    {
        _accountingService = accountingService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "CONTABILIDAD.ASIENTO.VER")]
    public async Task<IActionResult> Index(Guid? periodId)
    {
        var vm = await _accountingService.GetAsientoIndexContextAsync(periodId);
        return View(vm);
    }

    [Authorize(Policy = "CONTABILIDAD.ASIENTO.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var asientoContable = await _accountingService.GetEntryByIdAsync(id.Value);
        if (asientoContable == null) return NotFound();

        return View(asientoContable);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.ASIENTO.CREAR")]
    public async Task<IActionResult> Post(Guid id)
    {
        var result = await _accountingService.PostEntryAsync(id);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.ASIENTO.REVERTIR")]
    public async Task<IActionResult> Reverse(Guid id, string reason)
    {
        if (string.IsNullOrEmpty(reason))
        {
            TempData["Error"] = "Debe proporcionar un motivo para la reversión.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var result = await _accountingService.ReverseEntryAsync(id, reason);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "CONTABILIDAD.ASIENTO.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _accountingService.GetAsientoCreateContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.ASIENTO.CREAR")]
    public async Task<IActionResult> Create(AsientoCreateViewModel vm)
    {
        var asientoContable = vm.Entry;

        ModelState.Remove("Entry.Entidad");
        ModelState.Remove("Entry.Periodo");
        ModelState.Remove("Entry.TipoComprobante");
        ModelState.Remove("Entry.EntidadId");

        if (vm.Entry.AsientoDetalles.Count > 0)
        {
            asientoContable.EntidadId = _entidadProvider.CurrentEntidadId;
            asientoContable.CreadoPor = _entidadProvider.CurrentUsuarioId;

            var result = await _accountingService.CreateEntryAsync(asientoContable);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Details), new { id = result.Entry?.Id });
            }
            ModelState.AddModelError("", result.Message);
        }

        var contextVm = await _accountingService.GetAsientoCreateContextAsync(asientoContable);
        return View(contextVm);
    }

    [Authorize(Policy = "CONTABILIDAD.ASIENTO.CREAR")]
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var entry = await _accountingService.GetEntryByIdAsync(id.Value);
        if (entry == null) return NotFound();

        if (entry.Estado == "CONTABILIZADO")
        {
            TempData["Error"] = "No se puede eliminar un asiento ya contabilizado.";
            return RedirectToAction(nameof(Index));
        }

        return View(entry);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.ASIENTO.CREAR")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var result = await _accountingService.DeleteDraftEntryAsync(id);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Index));
    }
}
