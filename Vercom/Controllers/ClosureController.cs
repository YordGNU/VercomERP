using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class ClosureController : Controller
{
    private readonly AppDbContext _context;
    private readonly IClosureService _closureService;
    private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

    public ClosureController(AppDbContext context, IClosureService closureService)
    {
        _context = context;
        _closureService = closureService;
    }

    [Authorize(Policy = "ACC_CLOSE_PERIOD")]
    public async Task<IActionResult> Yearly()
    {
        var year = (short)DateTime.Now.Year;
        // Simular cálculo de resultados
        var ingresos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == CurrentEntidadId && d.Asiento.Periodo.Anio == year && d.Cuenta.Clase == "INGRESOS")
            .SumAsync(d => d.Haber - d.Debe);

        var gastos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == CurrentEntidadId && d.Asiento.Periodo.Anio == year && d.Cuenta.Clase == "GASTOS")
            .SumAsync(d => d.Debe - d.Haber);

        ViewBag.Year = year;
        ViewBag.Profit = ingresos - gastos;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ACC_CLOSE_PERIOD")]
    public async Task<IActionResult> ExecuteYearly(short year)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var result = await _closureService.CloseFiscalYearAsync(CurrentEntidadId, year, userId);

        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Yearly));
    }
}
