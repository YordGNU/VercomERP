using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Security;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class FichaCostoController : Controller
{
    private readonly IProductionService _productionService;
    private readonly IEntidadProvider _entidadProvider;

    public FichaCostoController(IProductionService productionService, IEntidadProvider entidadProvider)
    {
        _productionService = productionService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "PRODUCCION.FICHA.VER")]
    public async Task<IActionResult> Index()
    {
        var fichas = await _productionService.GetCostSheetsAsync();
        return View(fichas);
    }

    [Authorize(Policy = "PRODUCCION.FICHA.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();
        var ficha = await _productionService.GetCostSheetByIdAsync(id.Value);
        if (ficha == null) return NotFound();
        return View(ficha);
    }

    [Authorize(Policy = "PRODUCCION.FICHA.CREAR")]
    public async Task<IActionResult> Create(Guid? productoId)
    {
        var vm = await _productionService.GetCostSheetCreateContextAsync();
        if (productoId.HasValue) vm.CostSheet.ProductoId = productoId.Value;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PRODUCCION.FICHA.CREAR")]
    public async Task<IActionResult> Create([Bind(Prefix = "CostSheet")] FichaCosto fichaCosto)
    {
        ModelState.Remove("CostSheet.Producto");
        ModelState.Remove("CostSheet.Entidad");

        if (ModelState.IsValid)
        {
            fichaCosto.EntidadId = _entidadProvider.CurrentEntidadId;
            fichaCosto.CreadoPor = _entidadProvider.CurrentUsuarioId;

            var result = await _productionService.CreateCostSheetAsync(fichaCosto);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }
        var vm = await _productionService.GetCostSheetCreateContextAsync(fichaCosto);
        return View(vm);
    }
}
