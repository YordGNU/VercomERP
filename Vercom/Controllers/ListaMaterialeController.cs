using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class ListaMaterialeController : Controller
{
    private readonly IProductionService _productionService;

    public ListaMaterialeController(IProductionService productionService)
    {
        _productionService = productionService;
    }

    [Authorize(Policy = "PRODUCCION.BOM.VER")]
    public async Task<IActionResult> Index()
    {
        var boms = await _productionService.GetBomsAsync();
        return View(boms);
    }

    [Authorize(Policy = "PRODUCCION.BOM.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();
        var bom = await _productionService.GetBomByIdAsync(id.Value);
        if (bom == null) return NotFound();
        return View(bom);
    }

    [Authorize(Policy = "PRODUCCION.BOM.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _productionService.GetBomCreateContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PRODUCCION.BOM.CREAR")]
    public async Task<IActionResult> Create([Bind(Prefix = "Bom")] ListaMateriale bom)
    {
        ModelState.Remove("Bom.ProductoTerminado");

        if (ModelState.IsValid)
        {
            var result = await _productionService.CreateBomAsync(bom);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }
        var vm = await _productionService.GetBomCreateContextAsync(bom);
        return View(vm);
    }
}
