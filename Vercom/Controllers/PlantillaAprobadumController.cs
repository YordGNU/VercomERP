using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public async Task<IActionResult> Index(Guid? sucursalId, string? search)
    {
        var vm = await _hrService.GetPlantillaStatusAsync(sucursalId, search);
        var sucursales = await _adminService.GetSucursalesAsync();
        ViewBag.SucursalId = new SelectList(sucursales.Select(c => new { Id = c.Id, DisplayText = $"{c.Codigo} - {c.Nombre}" }), "Id", "DisplayText");
        ViewBag.CurrentSearch = search;
        return View(vm);
    }

    [Authorize(Policy = "RRHH.CARGO.CREAR")]
    public async Task<IActionResult> Create()
    {
        var cargos = await _hrService.GetCargosAsync();
        ViewBag.CargoId = new SelectList(cargos.Select(c => new { Id = c.Id, DisplayText = $"{c.Codigo} - {c.Nombre}" }), "Id", "DisplayText");

        var sucursales = await _adminService.GetSucursalesAsync();
        ViewBag.SucursalId = new SelectList(sucursales.Select(c => new { Id = c.Id, DisplayText = $"{c.Codigo} - {c.Nombre}" }), "Id", "DisplayText");

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
        var cargos = await _hrService.GetCargosAsync();
        ViewBag.CargoId = new SelectList(cargos.Select(c => new { Id = c.Id, DisplayText = $"{c.Codigo} - {c.Nombre}" }), "Id", "DisplayText");

        var sucursales = await _adminService.GetSucursalesAsync();
        ViewBag.SucursalId = new SelectList(sucursales.Select(c => new { Id = c.Id, DisplayText = $"{c.Codigo} - {c.Nombre}" }), "Id", "DisplayText");

        return View(entry);
    }

    [Authorize(Policy = "RRHH.CARGO.EDITAR")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var entry = await _hrService.GetPlantillaEntryByIdAsync(id);
        if (entry == null) return NotFound();

        var cargos = await _hrService.GetCargosAsync();
        ViewBag.CargoId = new SelectList(cargos.Select(c => new { Id = c.Id, DisplayText = $"{c.Codigo} - {c.Nombre}" }), "Id", "DisplayText", entry.CargoId);
        var sucursales = await _adminService.GetSucursalesAsync();
        ViewBag.SucursalId = new SelectList(sucursales.Select(c => new { Id = c.Id, DisplayText = $"{c.Codigo} - {c.Nombre}" }), "Id", "DisplayText", entry.SucursalId);
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
        var cargos = await _hrService.GetCargosAsync();
        ViewBag.CargoId = new SelectList(cargos.Select(c => new { Id = c.Id, DisplayText = $"{c.Codigo} - {c.Nombre}" }), "Id", "DisplayText", entry.CargoId);
        var sucursales = await _adminService.GetSucursalesAsync();
        ViewBag.SucursalId = new SelectList(sucursales.Select(c => new { Id = c.Id, DisplayText = $"{c.Codigo} - {c.Nombre}" }), "Id", "DisplayText", entry.SucursalId);
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
