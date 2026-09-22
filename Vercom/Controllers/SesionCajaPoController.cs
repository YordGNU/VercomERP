using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Security;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class SesionCajaPoController : Controller
{
    private readonly IPosService _posService;
    private readonly IEntidadProvider _entidadProvider;

    public SesionCajaPoController(IPosService posService, IEntidadProvider entidadProvider)
    {
        _posService = posService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "POS.CONFIGURACION.VER")]
    public async Task<IActionResult> Index()
    {
        var items = await _posService.GetSessionsAsync();
        return View(items);
    }

    [Authorize(Policy = "POS.CONFIGURACION.VER")]
    public async Task<IActionResult> Details(Guid id)
    {
        var item = await _posService.GetSessionByIdAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "POS.CONFIGURACION.CREAR")]
    public async Task<IActionResult> Close(Guid id, decimal amount, string notes)
    {
        var result = await _posService.CloseSessionAsync(id, amount, notes, _entidadProvider.CurrentUsuarioId);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Index));
    }
}
