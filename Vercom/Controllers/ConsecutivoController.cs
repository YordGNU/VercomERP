using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize(Policy = "ADMIN.CONSECUTIVO.VER")]
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
    public async Task<IActionResult> Details(int id)
    {
        var item = await _adminService.GetConsecutivoByIdAsync(id);
        if (item == null) return NotFound();
        return View(item);
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
        ModelState.Remove("Consecutivo.EntidadId");

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

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _adminService.GetConsecutivoFormContextAsync(id);
        if (vm.Consecutivo == null || vm.Consecutivo.Id == 0) return NotFound();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ConsecutivoFormViewModel vm)
    {
        if (id != vm.Consecutivo.Id) return BadRequest();
        var entry = vm.Consecutivo;
        ModelState.Remove("Consecutivo.Entidad");
        ModelState.Remove("Consecutivo.Sucursal");
        ModelState.Remove("Consecutivo.EntidadId");

        if (ModelState.IsValid)
        {
            var result = await _adminService.SaveConsecutivoAsync(entry);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _adminService.GetConsecutivoFormContextAsync(id);
        contextVm.Consecutivo = entry;
        return View(contextVm);
    }
}