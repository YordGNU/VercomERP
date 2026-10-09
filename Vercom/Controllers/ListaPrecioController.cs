using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class ListaPrecioController : Controller
{
    private readonly IInventoryService _inventoryService;

    public ListaPrecioController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [Authorize(Policy = "INVENTARIO.LISTA_PRECIO.VER")]
    public async Task<IActionResult> Index()
    {
        var lists = await _inventoryService.GetPriceListsAsync();
        return View(lists);
    }

    [Authorize(Policy = "INVENTARIO.LISTA_PRECIO.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();
        var list = await _inventoryService.GetPriceListByIdAsync(id.Value);
        if (list == null) return NotFound();
        return View(list);
    }

    [Authorize(Policy = "INVENTARIO.LISTA_PRECIO.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _inventoryService.GetPriceListFormContextAsync();
        return View(vm);
    }

    private void LimpiarModelStateServidor()
    {
        var raiz = "ListaPrecio.";
        var detallePrefijo = raiz + "ListaPrecioDetalles[";

        ModelState.Remove(raiz + "Entidad");
        ModelState.Remove(raiz + "EntidadId");
        ModelState.Remove(raiz + "Id");
        ModelState.Remove(raiz + "Clientes");
        ModelState.Remove(raiz + "ListaPrecioDetalles");
        ModelState.Remove(raiz + "Canal");

        var navsClienteServidor = new[]
        {
            ".ListaPrecioId",
            ".ListaPrecio",
            ".Producto",
            ".Id"
        };

        foreach (var key in ModelState.Keys.ToList())
        {
            if (!key.StartsWith(detallePrefijo, StringComparison.OrdinalIgnoreCase)) continue;
            if (navsClienteServidor.Any(s => key.EndsWith(s, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.Remove(key);
            }
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.LISTA_PRECIO.CREAR")]
    public async Task<IActionResult> Create(ListaPrecioFormViewModel vm)
    {
        var priceList = vm.ListaPrecio;

        LimpiarModelStateServidor();

        if (ModelState.IsValid)
        {
            var result = await _inventoryService.CreatePriceListAsync(priceList);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }

    [Authorize(Policy = "INVENTARIO.LISTA_PRECIO.EDITAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var list = await _inventoryService.GetPriceListByIdAsync(id.Value);
        if (list == null) return NotFound();

        var vm = await _inventoryService.GetPriceListFormContextAsync(list);
        return View(vm);
    }

    [HttpGet]
    [Authorize(Policy = "INVENTARIO.LISTA_PRECIO.EDITAR")]
    public async Task<IActionResult> BuscarProductos(string? search, CancellationToken cancellationToken)
    {
        var productos = await _inventoryService.SearchPriceListProductsAsync(search, cancellationToken);
        return Json(new
        {
            results = productos.Select(p => new
            {
                id = p.Id,
                text = $"{p.Codigo} - {p.Nombre}",
                price = p.PrecioVentaActual
            })
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.LISTA_PRECIO.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, ListaPrecioFormViewModel vm)
    {
        var priceList = vm.ListaPrecio;
        if (id != priceList.Id) return NotFound();

        LimpiarModelStateServidor();

        if (ModelState.IsValid)
        {
            var result = await _inventoryService.UpdatePriceListAsync(priceList);
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
