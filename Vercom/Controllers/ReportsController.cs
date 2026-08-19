using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IReportingService _reportingService;
    private readonly IFinancialReportService _financialReportService;

    public ReportsController(IReportingService reportingService, IFinancialReportService financialReportService)
    {
        _reportingService = reportingService;
        _financialReportService = financialReportService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Audit(Guid? userId, DateTime? start, DateTime? end)
    {
        var report = await _reportingService.GetUserAuditReportAsync(userId, start ?? DateTime.Now.AddDays(-30), end ?? DateTime.Now);
        return View(report);
    }

    public async Task<IActionResult> FinancialStatement(Guid periodId)
    {
        var entidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        var balance = await _financialReportService.GetBalanceGeneralAsync(entidadId, periodId);
        return View(balance);
    }

    [HttpPost]
    public async Task<IActionResult> GeneratePackage(Guid periodId)
    {
        var entidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        var result = await _reportingService.GenerateMonthlyPackageAsync(entidadId, periodId);
        TempData["Success"] = $"Paquete generado: {result}";
        return RedirectToAction(nameof(Index));
    }
}
