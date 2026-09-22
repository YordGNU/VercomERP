using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class CentroCostoController : Controller
{
    private readonly IAccountingService _accountingService;

    public CentroCostoController(IAccountingService accountingService)
    {
        _accountingService = accountingService;
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.VER")]
    public async Task<IActionResult> Index()
    {
        var items = await _accountingService.GetCostCentersAsync();
        return View(items);
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _accountingService.GetCostCenterFormContextAsync();
        return View(vm);
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.EDITAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var centrocosto = await _accountingService.GetCostCenterByIdAsync(id.Value);
        if (centrocosto == null) return NotFound();

        var vm = await _accountingService.GetCostCenterFormContextAsync(centrocosto);
        return View(vm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.PRODUCTO.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, CentroCostoFormViewModel vm)
    {
        var centroCosto = vm.CentroCosto;
        if (id != centroCosto.Id) return NotFound();

        ModelState.Remove("CentroCosto.Entidad");
        ModelState.Remove("CentroCosto.Sucursal");

        if (ModelState.IsValid)
        {
            var result = await _accountingService.UpdateCostCenterAsync(centroCosto);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CUENTA.CREAR")]
    public async Task<IActionResult> Create(CentroCostoFormViewModel vm)
    {
        var item = vm.CentroCosto;
        ModelState.Remove("CentroCosto.Entidad");
        ModelState.Remove("CentroCosto.Sucursal");

        if (ModelState.IsValid)
        {
            var result = await _accountingService.CreateCostCenterAsync(item);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }
}
