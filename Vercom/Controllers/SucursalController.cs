using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class SucursalController : Controller
{
    private readonly IAdminService _adminService;

    public SucursalController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
    public async Task<IActionResult> Index()
    {
        var items = await _adminService.GetSucursalesAsync();
        return View(items);
    }

    [Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
    public IActionResult Create() => View(new Sucursal { Activo = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
    public async Task<IActionResult> Create(Sucursal sucursal)
    {
        ModelState.Remove("Entidad");
        if (ModelState.IsValid)
        {
            var result = await _adminService.CreateSucursalAsync(sucursal);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        return View(sucursal);
    }
}
