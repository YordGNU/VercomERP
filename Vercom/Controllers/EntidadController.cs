using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.Security;

namespace Vercom.Controllers;

[Authorize(Roles = "ADMINISTRADOR")]
public class EntidadController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IEntidadProvider _entidadProvider;

    public EntidadController(IAdminService adminService, IEntidadProvider entidadProvider)
    {
        _adminService = adminService;
        _entidadProvider = entidadProvider;
    }

    private bool IsMasterUser => User.Identity?.Name == "master";

    public async Task<IActionResult> Index()
    {
        if (!IsMasterUser)
        {
            return RedirectToAction(nameof(Details), new { id = _entidadProvider.CurrentEntidadId });
        }
        var entidades = await _adminService.GetEntidadesAsync(true);
        return View(entidades);
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        var targetId = id ?? _entidadProvider.CurrentEntidadId;
        var entidad = await _adminService.GetEntidadByIdAsync(targetId, IsMasterUser, _entidadProvider.CurrentEntidadId);

        if (entidad == null) return Forbid();

        return View(entidad);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid? id)
    {
        var targetId = id ?? _entidadProvider.CurrentEntidadId;
        var entidad = await _adminService.GetEntidadByIdAsync(targetId, IsMasterUser, _entidadProvider.CurrentEntidadId);

        if (entidad == null) return Forbid();

        return View(entidad);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Entidad entidad)
    {
        if (id != entidad.Id) return NotFound();
        if (!IsMasterUser && id != _entidadProvider.CurrentEntidadId) return Forbid();

        if (ModelState.IsValid)
        {
            var result = await _adminService.UpdateEntidadAsync(entidad);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Details), new { id = entidad.Id });
            }
            ModelState.AddModelError("", result.Message);
        }
        return View(entidad);
    }
}
