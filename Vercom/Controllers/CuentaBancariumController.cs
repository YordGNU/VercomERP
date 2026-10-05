using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class CuentaBancariumController : Controller
{
    private readonly ICashBankService _cashBankService;

    public CuentaBancariumController(ICashBankService cashBankService)
    {
        _cashBankService = cashBankService;
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.VER")]
    public async Task<IActionResult> Index()
    {
        var accounts = await _cashBankService.GetBankAccountsAsync();
        return View(accounts);
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();
        var account = await _cashBankService.GetBankAccountByIdAsync(id.Value);
        if (account == null) return NotFound();
        return View(account);
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _cashBankService.GetBankAccountFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CUENTA.CREAR")]
    public async Task<IActionResult> Create(BankAccountFormViewModel vm)
    {
        var account = vm.BankAccount;
        ModelState.Remove("BankAccount.Entidad");
        ModelState.Remove("BankAccount.CuentaContable");
        if (ModelState.IsValid)
        {
            var result = await _cashBankService.CreateBankAccountAsync(account);
            if (result.Succeeded)
            {
                return Json(new
                {
                    success = true,
                    message = result.Message ?? "Cuenta bancaria creada correctamente.",
                    redirectUrl = Url.Action(nameof(Index))
                });
            }

            return Json(new { success = false, message = result.Message ?? "No se pudo crear la cuenta bancaria." });
        }

        return Json(new
        {
            success = false,
            message = "Revise los campos señalados.",
            errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray()
        });
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.EDITAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var account = await _cashBankService.GetBankAccountByIdAsync(id.Value);
        if (account == null) return NotFound();

        var vm = await _cashBankService.GetBankAccountFormContextAsync(account);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CUENTA.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, BankAccountFormViewModel vm)
    {
        var account = vm.BankAccount;
        if (id != account.Id) return Json(new { success = false, message = "La cuenta bancaria no corresponde al registro seleccionado." });

        ModelState.Remove("BankAccount.Entidad");
        ModelState.Remove("BankAccount.CuentaContable");

        if (ModelState.IsValid)
        {
            var result = await _cashBankService.UpdateBankAccountAsync(account);
            if (result.Succeeded)
            {
                return Json(new
                {
                    success = true,
                    message = result.Message ?? "Cuenta bancaria actualizada correctamente.",
                    redirectUrl = Url.Action(nameof(Index))
                });
            }

            return Json(new { success = false, message = result.Message ?? "No se pudo actualizar la cuenta bancaria." });
        }

        return Json(new
        {
            success = false,
            message = "Revise los campos señalados.",
            errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray()
        });
    }
}
