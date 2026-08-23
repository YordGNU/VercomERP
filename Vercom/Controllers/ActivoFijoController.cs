using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class ActivoFijoController : Controller
{
    private readonly IFixedAssetService _assetService;

    public ActivoFijoController(IFixedAssetService assetService)
    {
        _assetService = assetService;
    }

    [Authorize(Policy = "CONTABILIDAD.ACTIVO_FIJO.VER")]
    public async Task<IActionResult> Index()
    {
        var assets = await _assetService.GetAssetsAsync();
        return View(assets);
    }

    [Authorize(Policy = "CONTABILIDAD.ACTIVO_FIJO.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();
        var asset = await _assetService.GetAssetByIdAsync(id.Value);
        if (asset == null) return NotFound();
        return View(asset);
    }

    [Authorize(Policy = "CONTABILIDAD.ACTIVO_FIJO.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _assetService.GetAssetFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.ACTIVO_FIJO.CREAR")]
    public async Task<IActionResult> Create(AssetFormViewModel vm)
    {
        var asset = vm.Asset;

        ModelState.Remove("Asset.Entidad");
        ModelState.Remove("Asset.Sucursal");
        ModelState.Remove("Asset.CuentaActivo");
        ModelState.Remove("Asset.CuentaDepreciacion");
        ModelState.Remove("Asset.CuentaGastoDep");
        ModelState.Remove("Asset.Estado");
        ModelState.Remove("Asset.MetodoDepreciacion");

        if (ModelState.IsValid)
        {
            asset.Estado = "ACTIVO";
            asset.MetodoDepreciacion = "LINEA_RECTA";
            var result = await _assetService.CreateAssetAsync(asset);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }

        var contextVm = await _assetService.GetAssetFormContextAsync(asset);
        return View(contextVm);
    }

    [Authorize(Policy = "CONTABILIDAD.ACTIVO_FIJO.EDITAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var asset = await _assetService.GetAssetByIdAsync(id.Value);
        if (asset == null) return NotFound();

        var vm = await _assetService.GetAssetFormContextAsync(asset);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.ACTIVO_FIJO.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, AssetFormViewModel vm)
    {
        var asset = vm.Asset;
        if (id != asset.Id) return NotFound();

        ModelState.Remove("Asset.Entidad");
        ModelState.Remove("Asset.Sucursal");
        ModelState.Remove("Asset.CuentaActivo");
        ModelState.Remove("Asset.CuentaDepreciacion");
        ModelState.Remove("Asset.CuentaGastoDep");

        if (ModelState.IsValid)
        {
            var result = await _assetService.UpdateAssetAsync(asset);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }

        var contextVm = await _assetService.GetAssetFormContextAsync(asset);
        return View(contextVm);
    }

    [Authorize(Policy = "CONTABILIDAD.ACTIVO_FIJO.ELIMINAR")]
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();
        var asset = await _assetService.GetAssetByIdAsync(id.Value);
        if (asset == null) return NotFound();
        return View(asset);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.ACTIVO_FIJO.ELIMINAR")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var result = await _assetService.DeleteAssetAsync(id);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Index));
    }
}
