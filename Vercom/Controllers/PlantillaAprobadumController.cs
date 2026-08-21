using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class PlantillaAprobadumController : Controller
{
    private readonly IHRService _hrService;

    public PlantillaAprobadumController(IHRService hrService)
    {
        _hrService = hrService;
    }

    [Authorize(Policy = "RRHH.REPORTE.VER")]
    public async Task<IActionResult> Index()
    {
        var vm = await _hrService.GetPlantillaStatusAsync();
        return View(vm);
    }
}
