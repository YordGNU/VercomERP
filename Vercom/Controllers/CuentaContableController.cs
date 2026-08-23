using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class CuentaContableController : Controller
{
    private readonly IAccountingService _accountingService;

    public CuentaContableController(IAccountingService accountingService)
    {
        _accountingService = accountingService;
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.VER")]
    public async Task<IActionResult> Index()
    {
        var accounts = await _accountingService.GetAccountsAsync();
        return View(accounts);
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();
        var account = await _accountingService.GetAccountByIdAsync(id.Value);
        if (account == null) return NotFound();
        return View(account);
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _accountingService.GetAccountFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CUENTA.CREAR")]
    public async Task<IActionResult> Create(AccountFormViewModel vm)
    {
        var account = vm.Account;

        ModelState.Remove("Account.Entidad");
        ModelState.Remove("Account.CuentaPadre");

        if (ModelState.IsValid)
        {
            var result = await _accountingService.CreateAccountAsync(account);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }

        var contextVm = await _accountingService.GetAccountFormContextAsync(account);
        return View(contextVm);
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.EDITAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var account = await _accountingService.GetAccountByIdAsync(id.Value);
        if (account == null) return NotFound();

        var vm = await _accountingService.GetAccountFormContextAsync(account);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CUENTA.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, AccountFormViewModel vm)
    {
        var account = vm.Account;
        if (id != account.Id) return NotFound();

        ModelState.Remove("Account.Entidad");
        ModelState.Remove("Account.CuentaPadre");

        if (ModelState.IsValid)
        {
            var result = await _accountingService.UpdateAccountAsync(account);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }

        var contextVm = await _accountingService.GetAccountFormContextAsync(account);
        return View(contextVm);
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.ELIMINAR")]
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();
        var account = await _accountingService.GetAccountByIdAsync(id.Value);
        if (account == null) return NotFound();
        return View(account);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CUENTA.ELIMINAR")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var result = await _accountingService.DeleteAccountAsync(id);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Index));
    }
}
