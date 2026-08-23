using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class UsuarioController : Controller
{
    private readonly IAuthService _authService;

    public UsuarioController(IAuthService authService)
    {
        _authService = authService;
    }

    [Authorize(Policy = "SEGURIDAD.USUARIO.VER")]
    public async Task<IActionResult> Index()
    {
        var users = await _authService.GetUsersAsync();
        return View(users);
    }

    [Authorize(Policy = "SEGURIDAD.USUARIO.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var user = await _authService.GetUserByIdAsync(id.Value);
        if (user == null) return NotFound();

        return View(user);
    }

    [Authorize(Policy = "SEGURIDAD.USUARIO.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _authService.GetUserFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SEGURIDAD.USUARIO.CREAR")]
    public async Task<IActionResult> Create(UserFormViewModel vm)
    {
        var usuario = vm.Usuario;
        ModelState.Remove("Usuario.Entidad");

        if (ModelState.IsValid)
        {
            var result = await _authService.CreateUserAsync(usuario, vm.Password ?? "");
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }

        var contextVm = await _authService.GetUserFormContextAsync(usuario);
        return View(contextVm);
    }

    [Authorize(Policy = "SEGURIDAD.USUARIO.EDITAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var user = await _authService.GetUserByIdAsync(id.Value);
        if (user == null) return NotFound();

        var vm = await _authService.GetUserFormContextAsync(user);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SEGURIDAD.USUARIO.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, UserFormViewModel vm)
    {
        var usuario = vm.Usuario;
        if (id != usuario.Id) return NotFound();

        ModelState.Remove("Usuario.Entidad");

        if (ModelState.IsValid)
        {
            var result = await _authService.UpdateUserAsync(usuario);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }

        var contextVm = await _authService.GetUserFormContextAsync(usuario);
        return View(contextVm);
    }

    [HttpPost]
    [Authorize(Policy = "SEGURIDAD.USUARIO.EDITAR")]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var result = await _authService.ToggleUserStatusAsync(id);
        if (result.Succeeded) TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = "SEGURIDAD.USUARIO.EDITAR")]
    public async Task<IActionResult> ResetPassword(Guid id, string newPassword)
    {
        var result = await _authService.ResetUserPasswordAsync(id, newPassword);
        if (result.Succeeded) TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }
}
