using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Security;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class EntidadController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IEntidadProvider _entidadProvider;

    public EntidadController(IAdminService adminService, IEntidadProvider entidadProvider)
    {
        _adminService = adminService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Roles = "MASTER,ADMINISTRADOR")]
    public async Task<IActionResult> Index()
    {
        if (!_entidadProvider.IsMaster)
        {
            return RedirectToAction(nameof(Details), new { id = _entidadProvider.CurrentEntidadId });
        }
        var entidades = await _adminService.GetEntidadesAsync(true);
        return View(entidades);
    }

    [Authorize(Roles = "MASTER,ADMINISTRADOR")]
    public async Task<IActionResult> Pending()
    {
        if (!_entidadProvider.IsMaster) return Forbid();
        var pending = await _adminService.GetPendingEntidadesAsync();
        return View(pending);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "MASTER,ADMINISTRADOR")]
    public async Task<IActionResult> Approve(Guid id)
    {
        if (!_entidadProvider.IsMaster) return Forbid();
        var result = await _adminService.ApproveEntidadAsync(id, _entidadProvider.CurrentUsuarioId);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Pending));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        return View(new EntityRegistrationViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(EntityRegistrationViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var result = await _adminService.RegisterEntidadAsync(vm);
            if (result.Succeeded)
            {
                return View("RegisterSuccess");
            }
            ModelState.AddModelError("", result.Message);
        }
        return View(vm);
    }

    [Authorize(Roles = "MASTER,ADMINISTRADOR")]
    public async Task<IActionResult> Details(Guid? id)
    {
        var targetId = id ?? _entidadProvider.CurrentEntidadId;
        var entidad = await _adminService.GetEntidadByIdAsync(targetId, _entidadProvider.IsMaster, _entidadProvider.CurrentEntidadId);

        if (entidad == null) return Forbid();

        return View(entidad);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid? id)
    {
        var targetId = id ?? _entidadProvider.CurrentEntidadId;
        var entidad = await _adminService.GetEntidadByIdAsync(targetId, _entidadProvider.IsMaster, _entidadProvider.CurrentEntidadId);

        if (entidad == null) return Forbid();

        return View(entidad);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Entidad entidad)
    {
        if (id != entidad.Id) return NotFound();
        if (!_entidadProvider.IsMaster && id != _entidadProvider.CurrentEntidadId) return Forbid();

        if (ModelState.IsValid)
        {
            var result = await _adminService.UpdateEntidadAsync(entidad);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Details), new { id = entidad.Id }) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }
}
