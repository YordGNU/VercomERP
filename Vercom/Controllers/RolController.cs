using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize(Policy = "SEGURIDAD.ROL.VER")]
public class RolController : Controller
{
    private readonly IAuthService _authService;

    public RolController(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        var roles = await _authService.GetRolesAsync();
        return View(roles);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var rol = await _authService.GetRolByIdAsync(id.Value);
        if (rol == null) return NotFound();

        return View(rol);
    }

    [HttpGet]
    [Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
    public async Task<IActionResult> ManagePermissions(int id)
    {
        try
        {
            var vm = await _authService.GetRolPermissionsContextAsync(id);
            return View(vm);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
    public async Task<IActionResult> ManagePermissions(int id, int[] selectedPermissions)
    {
        var result = await _authService.UpdateRolPermissionsAsync(id, selectedPermissions);
        if (result.Succeeded)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", result.Message);
        var vm = await _authService.GetRolPermissionsContextAsync(id);
        return View(vm);
    }
}
