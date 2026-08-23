using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
public class ParametroSistemaController : Controller
{
    private readonly IAdminService _adminService;

    public ParametroSistemaController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _adminService.GetParametersAsync();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var vm = await _adminService.GetParameterFormContextAsync(id);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ParametroFormViewModel vm)
    {
        var entry = vm.Parametro;
        if (id ==  Guid.Empty) return NotFound();

        ModelState.Remove("Parametro.Entidad");

        if (ModelState.IsValid)
        {
            var result = await _adminService.SaveParameterAsync(entry);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        return View(vm);
    }
}
