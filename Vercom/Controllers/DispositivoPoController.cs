using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VERCOM.Services;
using VERCOM.ViewModels;

namespace VERCOM.Controllers;

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
        LimpiarNavegacion();

        if (ModelState.IsValid)
        {
            var result = await _posService.SaveDeviceAsync(device, device.EntidadId);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }

        var contextVm = await _posService.GetDeviceFormContextAsync(device);
        return View(contextVm);
    }

    // Los combos de caja/almacén dependen de la sucursal elegida: se recargan por AJAX.
    [HttpGet]
    [Authorize(Policy = "POS.CONFIGURACION.VER")]
    public async Task<IActionResult> Ubicaciones(Guid sucursalId)
    {
        var ubicaciones = await _posService.GetUbicacionesPorSucursalAsync(sucursalId);
        return Json(new
        {
            almacenes = ubicaciones.Almacenes.Select(x => new { value = x.Value, text = x.Text }),
            cajas = ubicaciones.Cajas.Select(x => new { value = x.Value, text = x.Text })
        });
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

        LimpiarNavegacion();

        if (ModelState.IsValid)
        {
            var result = await _posService.SaveDeviceAsync(device, device.EntidadId);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errores = string.Join(" ", ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .Where(m => !string.IsNullOrWhiteSpace(m)));
        return Json(new { success = false, message = errores.Length > 0 ? errores : "Errores de validación." });
    }

    // Las propiedades de navegación (Entidad, Sucursal, Almacén, Caja...) son no anulables y el
    // binder las marca como requeridas aunque el formulario solo envíe los Id: eso dejaba
    // ModelState inválido y el alta no guardaba nada (ni mostraba el motivo).
    private void LimpiarNavegacion()
    {
        const string raiz = "Dispositivo.";
        var navegaciones = new[]
        {
            "Entidad", "Sucursal", "Almacen", "Caja", "ApiCliente", "Auditoria", "FacturaVenta",
            "MovimientoInventarios", "PosRangoNumeracions", "PosSyncLogs", "PosVentaPendientes", "SesionCajaPo"
        };

        foreach (var clave in ModelState.Keys.Where(k => k.StartsWith(raiz, StringComparison.Ordinal)).ToList())
        {
            var propiedad = clave[raiz.Length..];
            if (navegaciones.Contains(propiedad, StringComparer.Ordinal)) ModelState.Remove(clave);
        }
    }
}