using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class PlantillaAprobadumController : Controller
{
    private readonly IHRService _hrService;
    private readonly IAdminService _adminService;

    public PlantillaAprobadumController(IHRService hrService, IAdminService adminService)
    {
        _hrService = hrService;
        _adminService = adminService;
    }

    [Authorize(Policy = "RRHH.REPORTE.VER")]
    public async Task<IActionResult> Index()
    {
        var vm = await _hrService.GetPlantillaStatusAsync();
        return View(vm);
    }

    [Authorize(Policy = "RRHH.CARGO.CREAR")]
    public async Task<IActionResult> Create()
    {
        ViewBag.CargoId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _hrService.GetCargosAsync(), "Id", "Nombre");
        ViewBag.SucursalId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _adminService.GetSucursalesAsync(), "Id", "Nombre");
        return View(new PlantillaAprobadum { VigenteDesde = DateOnly.FromDateTime(DateTime.Now) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.CARGO.CREAR")]
    public async Task<IActionResult> Create(PlantillaAprobadum entry)
    {
        ModelState.Remove("Cargo");
        ModelState.Remove("Entidad");
        if (ModelState.IsValid)
        {
            var result = await _hrService.CreatePlantillaEntryAsync(entry);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        ViewBag.CargoId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _hrService.GetCargosAsync(), "Id", "Nombre", entry.CargoId);
        ViewBag.SucursalId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _adminService.GetSucursalesAsync(), "Id", "Nombre", entry.SucursalId);
        return View(entry);
    }

    [Authorize(Policy = "RRHH.CARGO.EDITAR")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var entry = await _hrService.GetPlantillaEntryByIdAsync(id);
        if (entry == null) return NotFound();

        ViewBag.CargoId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _hrService.GetCargosAsync(), "Id", "Nombre", entry.CargoId);
        ViewBag.SucursalId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _adminService.GetSucursalesAsync(), "Id", "Nombre", entry.SucursalId);
        return View(entry);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.CARGO.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, PlantillaAprobadum entry)
    {
        if (id != entry.Id) return NotFound();
        ModelState.Remove("Cargo");
        ModelState.Remove("Entidad");
        if (ModelState.IsValid)
        {
            var result = await _hrService.UpdatePlantillaEntryAsync(entry);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        ViewBag.CargoId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _hrService.GetCargosAsync(), "Id", "Nombre", entry.CargoId);
        ViewBag.SucursalId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _adminService.GetSucursalesAsync(), "Id", "Nombre", entry.SucursalId);
        return View(entry);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.CARGO.EDITAR")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _hrService.DeletePlantillaEntryAsync(id);
        return Json(new { success = result.Succeeded, message = result.Message });
    }
}
