using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
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
    public async Task<IActionResult> Index()
    {
        var providers = await _commercialService.GetProvidersAsync();
        return View(providers);
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
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _commercialService.GetProviderFormContextAsync(provider);
        return View(contextVm);
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
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _commercialService.GetProviderFormContextAsync(provider);
        return View(contextVm);
    }
}
