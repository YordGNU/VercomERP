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

    [Authorize(Policy = "INVENTARIO.LISTA_PRECIO.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _inventoryService.GetPriceListFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.LISTA_PRECIO.CREAR")]
    public async Task<IActionResult> Create(ListaPrecioFormViewModel vm)
    {
        var priceList = vm.ListaPrecio;
        ModelState.Remove("ListaPrecio.Entidad");

        if (ModelState.IsValid)
        {
            var result = await _inventoryService.CreatePriceListAsync(priceList);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        return View(vm);
    }

    [Authorize(Policy = "INVENTARIO.LISTA_PRECIO.CREAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var list = await _inventoryService.GetPriceListByIdAsync(id.Value);
        if (list == null) return NotFound();

        var vm = await _inventoryService.GetPriceListFormContextAsync(list);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "INVENTARIO.LISTA_PRECIO.CREAR")]
    public async Task<IActionResult> Edit(Guid id, ListaPrecioFormViewModel vm)
    {
        var priceList = vm.ListaPrecio;
        if (id != priceList.Id) return NotFound();

        ModelState.Remove("ListaPrecio.Entidad");

        if (ModelState.IsValid)
        {
            var result = await _inventoryService.UpdatePriceListAsync(priceList);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        return View(vm);
    }
}
