using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize(Policy = "SEGURIDAD.ROL.VER")]
public class RolController : Controller
{
    private readonly IAuthService _authService;
    private readonly Security.IEntidadProvider _entidadProvider;

    public RolController(IAuthService authService, Security.IEntidadProvider entidadProvider)
    {
        _authService = authService;
        _entidadProvider = entidadProvider;
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
    public IActionResult Create()
    {
        return View(new Rol());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
    public async Task<IActionResult> Create(Rol rol)
    {
        if (ModelState.IsValid)
        {
            var result = await _authService.CreateRolAsync(rol);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }

    [HttpGet]
    [Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
    public async Task<IActionResult> Edit(int id)
    {
        var rol = await _authService.GetRolByIdAsync(id);
        if (rol == null) return NotFound();

        if (rol.EsSistema && !_entidadProvider.IsMaster)
        {
            TempData["Error"] = "No se pueden editar roles de sistema.";
            return RedirectToAction(nameof(Index));
        }

        return View(rol);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
    public async Task<IActionResult> Edit(int id, Rol rol)
    {
        if (id != rol.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var result = await _authService.UpdateRolAsync(rol);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }

    [HttpGet]
    [Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
    public async Task<IActionResult> ManagePermissions(int id)
    {
        try
        {
            var rol = await _authService.GetRolByIdAsync(id);
            if (rol == null) return NotFound();

            // Bloqueo: Solo el Master puede editar roles de sistema
            if (rol.EsSistema && !_entidadProvider.IsMaster)
            {
                TempData["Error"] = "Solo el Administrador Global puede modificar los permisos de los roles de sistema.";
                return RedirectToAction(nameof(Index));
            }

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
        var rol = await _authService.GetRolByIdAsync(id);
        if (rol == null) return NotFound();

        // Doble validación en el POST
        if (rol.EsSistema && !_entidadProvider.IsMaster)
        {
            return Forbid();
        }

        var result = await _authService.UpdateRolPermissionsAsync(id, selectedPermissions);
        if (result.Succeeded)
        {
            return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
        }

        return Json(new { success = false, message = result.Message });
    }
}
