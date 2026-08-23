using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
public class ConsecutivoController : Controller
{
    private readonly IAdminService _adminService;

    public ConsecutivoController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _adminService.GetConsecutivosAsync();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = await _adminService.GetConsecutivoFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ConsecutivoFormViewModel vm)
    {
        var entry = vm.Consecutivo;
        ModelState.Remove("Consecutivo.Entidad");
        ModelState.Remove("Consecutivo.Sucursal");

        if (ModelState.IsValid)
        {
            var result = await _adminService.SaveConsecutivoAsync(entry);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _adminService.GetConsecutivoFormContextAsync();
        contextVm.Consecutivo = entry;
        return View(contextVm);
    }
}
