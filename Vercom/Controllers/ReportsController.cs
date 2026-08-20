using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IReportingService _reportingService;
    private readonly IFinancialReportService _financialReportService;
    private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

    public ReportsController(AppDbContext context, IReportingService reportingService, IFinancialReportService financialReportService)
    {
        _context = context;
        _reportingService = reportingService;
        _financialReportService = financialReportService;
    }

    [Authorize(Policy = "REPORTES.GENERAR")]
    public async Task<IActionResult> Index()
    {
        ViewBag.Periods = await _context.PeriodoContables
            .Where(p => p.EntidadId == CurrentEntidadId)
            .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes)
            .ToListAsync();
        return View();
    }

    [Authorize(Policy = "REPORTES.GENERAR")]
    public async Task<IActionResult> FinancialStatement(Guid periodId, string type)
    {
        var period = await _context.PeriodoContables.FindAsync(periodId);
        if (period == null) return NotFound();

        if (type == "BALANCE")
        {
            var data = await _financialReportService.GetBalanceGeneralAsync(CurrentEntidadId, periodId);
            ViewBag.ReportName = "Balance General";
            return View("FinancialReport", data);
        }
        else
        {
            var data = await _financialReportService.GetEstadoResultadosAsync(CurrentEntidadId, periodId);
            ViewBag.ReportName = "Estado de Resultados";
            return View("FinancialReport", data);
        }
    }

    [Authorize(Policy = "REPORTES.PAQUETE.VER")]
    public async Task<IActionResult> Packages()
    {
        var packages = await _context.PaqueteInformacions
            .Include(p => p.Periodo)
            .Where(p => p.EntidadId == CurrentEntidadId)
            .OrderByDescending(p => p.GeneradoEn)
            .ToListAsync();
        return View(packages);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "REPORTES.PAQUETE.CREAR")]
    public async Task<IActionResult> GeneratePackage(Guid periodId)
    {
        var fileName = await _reportingService.GenerateMonthlyPackageAsync(CurrentEntidadId, periodId);
        TempData["Success"] = $"Paquete Informativo generado: {fileName}. Listo para descarga.";
        return RedirectToAction(nameof(Packages));
    }
}
