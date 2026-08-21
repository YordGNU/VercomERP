using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
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
        return View(items);
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
}
