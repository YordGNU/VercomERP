using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class ProductoController : Controller
{
    private readonly IInventoryService _inventoryService;

    public ProductoController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [Authorize(Policy = "INVENTARIO.PRODUCTO.VER")]
    public async Task<IActionResult> Index()
    {
        var products = await _inventoryService.GetCatalogAsync();
        return View(products);
    }

    [Authorize(Policy = "INVENTARIO.PRODUCTO.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();
        var product = await _inventoryService.GetProductByIdAsync(id.Value);
        if (product == null) return NotFound();
        return View(product);
    }

    [Authorize(Policy = "INVENTARIO.PRODUCTO.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _inventoryService.GetProductFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.PRODUCTO.CREAR")]
    public async Task<IActionResult> Create(ProductFormViewModel vm)
    {
        var producto = vm.Producto;

        ModelState.Remove("Producto.Entidad");
        ModelState.Remove("Producto.Familia");
        ModelState.Remove("Producto.UnidadMedida");
        ModelState.Remove("Producto.EntidadId");

        if (ModelState.IsValid)
        {
            var result = await _inventoryService.CreateProductAsync(producto);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _inventoryService.GetProductFormContextAsync(producto);
        return View(contextVm);
    }

    [Authorize(Policy = "INVENTARIO.PRODUCTO.EDITAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var product = await _inventoryService.GetProductByIdAsync(id.Value);
        if (product == null) return NotFound();

        var vm = await _inventoryService.GetProductFormContextAsync(product);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.PRODUCTO.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, ProductFormViewModel vm)
    {
        var producto = vm.Producto;
        if (id != producto.Id) return NotFound();

        ModelState.Remove("Producto.Entidad");
        ModelState.Remove("Producto.Familia");
        ModelState.Remove("Producto.UnidadMedida");
        ModelState.Remove("Producto.EntidadId");

        if (ModelState.IsValid)
        {
            var result = await _inventoryService.UpdateProductAsync(producto);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _inventoryService.GetProductFormContextAsync(producto);
        return View(contextVm);
    }

    [Authorize(Policy = "INVENTARIO.PRODUCTO.ELIMINAR")]
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();
        var product = await _inventoryService.GetProductByIdAsync(id.Value);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.PRODUCTO.ELIMINAR")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var result = await _inventoryService.DeleteProductAsync(id);
        if (result.Succeeded) return RedirectToAction(nameof(Index));

        TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
