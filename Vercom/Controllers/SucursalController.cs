using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize(Policy = "ADMIN.SUCURSAL.VER")]
public class SucursalController : Controller
{
    private readonly IAdminService _adminService;

    public SucursalController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index(string? search, string? tipo, string? estado, int page = 1)
    {
        bool? activo = estado switch
        {
            "activa" => true,
            "inactiva" => false,
            _ => null
        };

        var vm = await _adminService.GetSucursalesPagedAsync(search, tipo, activo, page, 12);
        return View(vm);
    }

    public async Task<IActionResult> Mapa()
    {
        var items = await _adminService.GetSucursalesAsync();
        return View(items);
    }

    public async Task<IActionResult> MapaData()
    {
        var items = await _adminService.GetSucursalesAsync();
        var data = items.Select(s => new
        {
            s.Id,
            s.Nombre,
            s.Codigo,
            Tipo = s.Tipo ?? "SIN TIPO",
            s.Direccion,
            s.Municipio,
            s.Provincia,
            s.Telefono,
            s.Activo,
            Lat = s.Latitud,
            Lng = s.Longitud
        });
        return Json(data);
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();
        var item = await _adminService.GetSucursalByIdAsync(id.Value);
        if (item == null) return NotFound();
        return View(item);
    }

    public IActionResult Create() => View(new Sucursal { Activo = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Sucursal sucursal)
    {
        ModelState.Remove("Entidad");
        if (ModelState.IsValid)
        {
            var result = await _adminService.CreateSucursalAsync(sucursal);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var item = await _adminService.GetSucursalByIdAsync(id.Value);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Sucursal sucursal)
    {
        if (id != sucursal.Id) return NotFound();

        ModelState.Remove("Entidad");
        if (ModelState.IsValid)
        {
            var result = await _adminService.UpdateSucursalAsync(sucursal);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
            ModelState.AddModelError("", result.Message);
        }
        return View(sucursal);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _adminService.DeleteSucursalAsync(id);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}