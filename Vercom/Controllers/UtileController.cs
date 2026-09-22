using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class UtileController : Controller
{
    private readonly IHRService _hrService;
    private readonly Security.IEntidadProvider _entidadProvider;

    public UtileController(IHRService hrService, Security.IEntidadProvider entidadProvider)
    {
        _hrService = hrService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "RRHH.UTILES.VER")]
    public async Task<IActionResult> Index()
    {
        var utiles = await _hrService.GetUtilesAsync();
        return View(utiles);
    }

    [Authorize(Policy = "RRHH.EMPLEADO.EDITAR")]
    public async Task<IActionResult> Assign(Guid? employeeId)
    {
        ViewBag.EmpleadoId = new SelectList(await _hrService.GetEmployeesAsync(), "Id", "NombreCompleto", employeeId);
        return View(new UtileResponsabilidad { EmpleadoId = employeeId ?? Guid.Empty, FechaEntrega = DateOnly.FromDateTime(DateTime.Now) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.EMPLEADO.EDITAR")]
    public async Task<IActionResult> Assign(UtileResponsabilidad utile)
    {
        ModelState.Remove("Empleado");
        ModelState.Remove("Entidad");
        if (ModelState.IsValid)
        {
            var result = await _hrService.AssignUtileAsync(utile);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action("File", "Empleado", new { id = utile.EmpleadoId }) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.EMPLEADO.EDITAR")]
    public async Task<IActionResult> Return(Guid id, DateOnly returnDate, string? observations)
    {
        var result = await _hrService.ReturnUtileAsync(id, returnDate, observations);
        return Json(new { success = result.Succeeded, message = result.Message });
    }
}
