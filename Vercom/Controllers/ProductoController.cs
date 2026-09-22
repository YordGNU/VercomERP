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

    [Authorize(Policy = "INVENTARIO.PRODUCTO.VER")]
    public async Task<IActionResult> Kardex(Guid id, Guid? almacenId)
    {
        var product = await _inventoryService.GetProductByIdAsync(id);
        if (product == null) return NotFound();

        var kardex = await _inventoryService.GetKardexByProductAsync(id, almacenId);
        ViewBag.Product = product;
        ViewBag.SelectedAlmacenId = almacenId;

        return View(kardex);
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
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Verifique los datos del formulario.", errors = errorList });
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
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
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

    // ============================================================
    // BÚSQUEDA POR CÓDIGO DE BARRAS (PARA POS/ALMACÉN)
    // ============================================================
    [HttpGet]
    [Authorize(Policy = "INVENTARIO.PRODUCTO.VER")]
    public async Task<IActionResult> SearchByBarcode(string barcode)
    {
        var catalog = await _inventoryService.GetCatalogAsync();
        var product = catalog.FirstOrDefault(p => p.CodigoBarras == barcode && p.Activo);

        if (product == null) return NotFound();

        return Json(new {
            id = product.Id,
            nombre = product.Nombre,
            codigo = product.Codigo,
            precio = product.PrecioVentaActual
        });
    }

    // ============================================================
    // DESACTIVACIÓN LÓGICA (PARA PRODUCTOS CON HISTORIAL)
    // ============================================================
    [HttpPost]
    [Authorize(Policy = "INVENTARIO.PRODUCTO.EDITAR")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var product = await _inventoryService.GetProductByIdAsync(id);
        if (product == null) return NotFound();

        product.Activo = false;
        var result = await _inventoryService.UpdateProductAsync(product);

        return Json(new { success = result.Succeeded, message = result.Message });
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