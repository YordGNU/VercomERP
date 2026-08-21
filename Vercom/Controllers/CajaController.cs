using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class CajaController : Controller
{
    private readonly ICashBankService _cashBankService;

    public CajaController(ICashBankService cashBankService)
    {
        _cashBankService = cashBankService;
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.VER")]
    public async Task<IActionResult> Index()
    {
        var cajas = await _cashBankService.GetCajasAsync();
        return View(cajas);
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();
        var caja = await _cashBankService.GetCajaByIdAsync(id.Value);
        if (caja == null) return NotFound();
        return View(caja);
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _cashBankService.GetCajaFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CUENTA.CREAR")]
    public async Task<IActionResult> Create(CajaFormViewModel vm)
    {
        var caja = vm.Caja;
        ModelState.Remove("Caja.Entidad");
        ModelState.Remove("Caja.Sucursal");
        ModelState.Remove("Caja.CuentaContable");

        if (ModelState.IsValid)
        {
            var result = await _cashBankService.CreateCajaAsync(caja);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _cashBankService.GetCajaFormContextAsync(caja);
        return View(contextVm);
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.EDITAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var caja = await _cashBankService.GetCajaByIdAsync(id.Value);
        if (caja == null) return NotFound();

        var vm = await _cashBankService.GetCajaFormContextAsync(caja);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CUENTA.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, CajaFormViewModel vm)
    {
        var caja = vm.Caja;
        if (id != caja.Id) return NotFound();

        ModelState.Remove("Caja.Entidad");
        ModelState.Remove("Caja.Sucursal");
        ModelState.Remove("Caja.CuentaContable");

        if (ModelState.IsValid)
        {
            var result = await _cashBankService.UpdateCajaAsync(caja);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _cashBankService.GetCajaFormContextAsync(caja);
        return View(contextVm);
    }
}
