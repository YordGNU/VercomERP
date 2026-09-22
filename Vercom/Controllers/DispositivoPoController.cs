using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class DispositivoPoController : Controller
{
    private readonly IPosService _posService;

    public DispositivoPoController(IPosService posService)
    {
        _posService = posService;
    }

    [Authorize(Policy = "POS.CONFIGURACION.VER")]
    public async Task<IActionResult> Index()
    {
        var items = await _posService.GetDevicesAsync();
        var openByCaja = (await _posService.GetSessionsAsync())
            .Where(s => s.Estado == "ABIERTA")
            .GroupBy(s => s.CajaId)
            .ToDictionary(g => g.Key, g => g.First());
        ViewBag.SesionesActivas = openByCaja;
        return View(items);
    }

    [Authorize(Policy = "POS.CONFIGURACION.VER")]
    public async Task<IActionResult> Details(Guid id)
    {
        var item = await _posService.GetDeviceByIdAsync(id);
        if (item == null) return NotFound();
        ViewBag.Sesiones = (await _posService.GetSessionsAsync())
            .Where(s => s.DispositivoPosId == id)
            .OrderByDescending(s => s.FechaApertura)
            .Take(10);
        return View(item);
    }

    [Authorize(Policy = "POS.CONFIGURACION.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _posService.GetDeviceFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "POS.CONFIGURACION.CREAR")]
    public async Task<IActionResult> Create(DispositivoPosFormViewModel vm)
    {
        var device = vm.Dispositivo;
        ModelState.Remove("Dispositivo.Entidad");
        ModelState.Remove("Dispositivo.Sucursal");

        if (ModelState.IsValid)
        {
            var result = await _posService.SaveDeviceAsync(device);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }

        var contextVm = await _posService.GetDeviceFormContextAsync(device);
        return View(contextVm);
    }

    [Authorize(Policy = "POS.CONFIGURACION.EDITAR")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var device = await _posService.GetDeviceByIdAsync(id);
        if (device == null) return NotFound();
        var vm = await _posService.GetDeviceFormContextAsync(device);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "POS.CONFIGURACION.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, DispositivoPosFormViewModel vm)
    {
        var device = vm.Dispositivo;
        if (id != device.Id) return NotFound();

        ModelState.Remove("Dispositivo.Entidad");
        ModelState.Remove("Dispositivo.Sucursal");

        if (ModelState.IsValid)
        {
            var result = await _posService.SaveDeviceAsync(device);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        return Json(new { success = false, message = "Errores de validación." });
    }
}