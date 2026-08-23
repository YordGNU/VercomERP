using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        ModelState.Remove("Producto.CuentaInventario");
        ModelState.Remove("Producto.CuentaCostoVenta");
        ModelState.Remove("Producto.CuentaIngreso");

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
        ModelState.Remove("Producto.CuentaInventario");
        ModelState.Remove("Producto.CuentaCostoVenta");
        ModelState.Remove("Producto.CuentaIngreso");

        if (ModelState.IsValid)
        {
            var result = await _inventoryService.UpdateProductAsync(producto);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _inventoryService.GetProductFormContextAsync(producto);
        return View(contextVm);
    }

    // ============================================================
    // ELIMINACIÓN INDIVIDUAL (AJAX)
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.PRODUCTO.ELIMINAR")]
    public async Task<IActionResult> Delete([FromBody] DeleteRequest request)
    {
        try
        {
            var result = await _inventoryService.DeleteAsync(request.Id);

            if (result)
            {
                return Json(new { success = true, message = "Producto eliminado correctamente." });
            }
            else
            {
                var producto = await _inventoryService.GetProductByIdAsync(request.Id);
                if (producto == null)
                    return Json(new { success = false, message = "Producto no encontrado." });

                return Json(new { success = false, message = "No se puede eliminar porque tiene movimientos de inventario." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Ocurrió un error al eliminar el producto." });
        }
    }

    // ============================================================
    // ELIMINACIÓN EN LOTE (AJAX)
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.PRODUCTO.ELIMINAR")]
    public async Task<IActionResult> DeleteSelected([FromBody] DeleteSelectedRequest request)
    {
        try
        {
            var result = await _inventoryService.DeleteSelectedAsync(request.Ids);

            if (result.Success)
            {
                return Json(new { success = true, message = result.Message, count = result.DeletedCount });
            }
            else
            {
                return Json(new { success = false, message = result.Message, failedIds = result.FailedIds });
            }
        }
        catch (Exception ex)
        {

            return Json(new { success = false, message = "Ocurrió un error al eliminar los productos." });
        }
    }
}

public class DeleteRequest
{
    public Guid Id { get; set; }
}

public class DeleteSelectedRequest
{
    public List<Guid> Ids { get; set; } = new();
}