using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Vercom.Controllers;

[Authorize(Policy = "INVENTARIO.PRODUCTO.VER")]
public sealed class ImportacionController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
