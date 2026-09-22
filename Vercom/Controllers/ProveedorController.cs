using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class ProveedorController : Controller
{
    private readonly ICommercialService _commercialService;

    public ProveedorController(ICommercialService commercialService)
    {
        _commercialService = commercialService;
    }

    [Authorize(Policy = "COMERCIAL.PROVEEDOR.VER")]
    public async Task<IActionResult> Index(string? search, string? tipoPersona, string? estado, int page = 1)
    {
        bool? activo = estado switch { "activa" => true, "inactiva" => false, _ => null };
        var vm = await _commercialService.GetProviderIndexAsync(search, tipoPersona, activo, page);
        return View(vm);
    }

    [Authorize(Policy = "COMERCIAL.PROVEEDOR.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();
        var provider = await _commercialService.GetProviderByIdAsync(id.Value);
        if (provider == null) return NotFound();
        return View(provider);
    }

    [Authorize(Policy = "COMERCIAL.PROVEEDOR.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _commercialService.GetProviderFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.PROVEEDOR.CREAR")]
    public async Task<IActionResult> Create(ProviderFormViewModel vm)
    {
        var provider = vm.Provider;
        ModelState.Remove("Provider.Entidad");

        if (ModelState.IsValid)
        {
            var result = await _commercialService.CreateProviderAsync(provider);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Verifique los datos del proveedor.", errors = errorList });
    }

    [Authorize(Policy = "COMERCIAL.PROVEEDOR.EDITAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var provider = await _commercialService.GetProviderByIdAsync(id.Value);
        if (provider == null) return NotFound();

        var vm = await _commercialService.GetProviderFormContextAsync(provider);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.PROVEEDOR.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, ProviderFormViewModel vm)
    {
        var provider = vm.Provider;
        if (id != provider.Id) return NotFound();

        ModelState.Remove("Provider.Entidad");

        if (ModelState.IsValid)
        {
            var result = await _commercialService.UpdateProviderAsync(provider);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }
}
