using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.Security;

namespace Vercom.Controllers;

[Authorize]
public class MovimientoBancarioController : Controller
{
    private readonly ICashBankService _cashBankService;
    private readonly IEntidadProvider _entidadProvider;

    public MovimientoBancarioController(ICashBankService cashBankService, IEntidadProvider entidadProvider)
    {
        _cashBankService = cashBankService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.VER")]
    public async Task<IActionResult> Index()
    {
        var items = await _cashBankService.GetBankMovementsAsync();
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CUENTA.CREAR")]
    public async Task<IActionResult> Create(MovimientoBancario movement)
    {
        ModelState.Remove("CuentaBancaria");
        ModelState.Remove("Asiento");

        if (ModelState.IsValid)
        {
            var result = await _cashBankService.CreateBankMovementAsync(movement);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        return View(movement);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CUENTA.EDITAR")]
    public async Task<IActionResult> Reconcile(Guid id)
    {
        var result = await _cashBankService.ReconcileBankMovementAsync(id, _entidadProvider.CurrentUsuarioId);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Index));
    }
}
