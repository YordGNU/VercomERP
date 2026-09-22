using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class TopePrecioMfpController : Controller
{
    private readonly ICommercialService _commercialService;

    public TopePrecioMfpController(ICommercialService commercialService)
    {
        _commercialService = commercialService;
    }

    [Authorize(Policy = "COMERCIAL.CLIENTE.VER")]
    public async Task<IActionResult> Index()
    {
        var items = await _commercialService.GetPriceLimitsAsync();
        return View(items);
    }

    [Authorize(Policy = "COMERCIAL.CLIENTE.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _commercialService.GetPriceLimitFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.CLIENTE.CREAR")]
    public async Task<IActionResult> Create(PriceLimitFormViewModel vm)
    {
        var limit = vm.Limit;
        ModelState.Remove("Limit.Producto");
        ModelState.Remove("Limit.Familia");

        if (ModelState.IsValid)
        {
            var result = await _commercialService.CreatePriceLimitAsync(limit);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Verifique los datos del tope.", errors = errorList });
    }

    [HttpGet]
    public async Task<IActionResult> GetTope(Guid productoId)
    {
        // Obtener el producto para saber su familia
        var context = (Vercom.Models.AppDbContext)HttpContext.RequestServices.GetService(typeof(Vercom.Models.AppDbContext));
        var prod = await context.Productos.FindAsync(productoId);
        if (prod == null) return Json(new { exists = false });

        var today = DateOnly.FromDateTime(DateTime.Now);
        var tope = await context.TopePrecioMfps
            .Where(t => (t.ProductoId == productoId || t.FamiliaId == prod.FamiliaId)
                        && t.VigenteDesde <= today && (t.VigenteHasta == null || t.VigenteHasta >= today))
            .OrderByDescending(t => t.ProductoId) // Priorizar producto sobre familia
            .FirstOrDefaultAsync();

        if (tope == null) return Json(new { exists = false });

        return Json(new { exists = true, precioMaximo = tope.PrecioMaximo });
    }
}
