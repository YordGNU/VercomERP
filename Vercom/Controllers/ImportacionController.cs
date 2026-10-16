using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VERCOM.Controllers;

[Authorize(Policy = "INVENTARIO.PRODUCTO.VER")]
public sealed class ImportacionController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
