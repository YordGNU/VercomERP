using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class PosVentaPendienteController : Controller
{
    private readonly IPosService _posService;

    public PosVentaPendienteController(IPosService posService)
    {
        _posService = posService;
    }

    [Authorize(Policy = "POS.CONFIGURACION.VER")]
    public async Task<IActionResult> Index(Guid? deviceId = null)
    {
        var items = await _posService.GetPendingSalesAsync();
        if (deviceId.HasValue && deviceId != Guid.Empty)
            items = items.Where(x => x.DispositivoPosId == deviceId.Value);
        ViewBag.DeviceId = deviceId;
        return View(items);
    }

    [Authorize(Policy = "POS.CONFIGURACION.VER")]
    public async Task<IActionResult> Details(Guid id)
    {
        var item = await _posService.GetPendingSaleByIdAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "POS.CONFIGURACION.CREAR")]
    public async Task<IActionResult> Process(Guid id)
    {
        var result = await _posService.ProcessPendingSaleAsync(id);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Index));
    }
}
