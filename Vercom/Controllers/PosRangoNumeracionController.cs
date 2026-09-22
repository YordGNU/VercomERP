using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class PosRangoNumeracionController : Controller
{
    private readonly IPosService _posService;

    public PosRangoNumeracionController(IPosService posService)
    {
        _posService = posService;
    }

    [Authorize(Policy = "POS.CONFIGURACION.VER")]
    public async Task<IActionResult> Index()
    {
        var items = await _posService.GetRangesAsync();
        return View(items);
    }
}