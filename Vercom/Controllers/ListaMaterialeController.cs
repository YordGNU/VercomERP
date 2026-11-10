using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VERCOM.Models;
using VERCOM.Services;

namespace VERCOM.Controllers;

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

    [Authorize(Policy = "PRODUCCION.BOM.EDITAR")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var bom = await _productionService.GetBomByIdAsync(id);
        if (bom == null) return NotFound();
        var vm = await _productionService.GetBomCreateContextAsync(bom);
        vm.Title = $"Nueva versión BOM: {bom.ProductoTerminado.Nombre}";
        return View("Create", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PRODUCCION.BOM.CREAR")]
    public async Task<IActionResult> Create([Bind(Prefix = "Bom")] ListaMateriale bom)
    {
        LimpiarNavegacionBom();

        if (ModelState.IsValid)
        {
            var result = await _productionService.CreateBomAsync(bom);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PRODUCCION.BOM.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, [Bind(Prefix = "Bom")] ListaMateriale bom)
    {
        bom.Id = id;
        LimpiarNavegacionBom();
        if (ModelState.IsValid)
        {
            var result = await _productionService.UpdateBomAsync(bom);
            if (result.Succeeded)
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            ModelState.AddModelError(string.Empty, result.Message);
        }

        var context = await _productionService.GetBomCreateContextAsync(bom);
        context.Title = "Editar versión de BOM";
        return View("Create", context);
    }

    private void LimpiarNavegacionBom()
    {
        ModelState.Remove("Bom.ProductoTerminado");
        foreach (var key in ModelState.Keys
                     .Where(k => k.StartsWith("Bom.ListaMaterialesDetalles[", StringComparison.Ordinal) &&
                                 (k.EndsWith(".ProductoInsumo", StringComparison.Ordinal) ||
                                  k.EndsWith(".ListaMateriales", StringComparison.Ordinal)))
                     .ToList())
        {
            ModelState.Remove(key);
        }
    }
}
