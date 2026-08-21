using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class TipoMovimientoController : Controller
{
    private readonly IInventoryService _inventoryService;

    public TipoMovimientoController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public async Task<IActionResult> Index()
    {
        var types = await _inventoryService.GetMovementTypesAsync();
        return View(types);
    }
}
