using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Security;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IIntelligenceService _intelligenceService;
    private readonly IAccountingService _accountingService;
    private readonly IExportService _exportService;
    private readonly IEntidadProvider _entidadProvider;

    public ReportsController(IIntelligenceService intelligenceService, IAccountingService accountingService,
        IExportService exportService, IEntidadProvider entidadProvider)
    {
        _intelligenceService = intelligenceService;
        _accountingService = accountingService;
        _exportService = exportService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "REPORTES.INDICADOR.VER")]
    public async Task<IActionResult> Index()
    {
        var periods = await _accountingService.GetPeriodsAsync();
        var vm = new ReportsIndexViewModel
        {
            Periods = new SelectList(periods.Select(p => new { p.Id, Display = $"{p.Mes}/{p.Anio}" }), "Id", "Display")
        };
        return View(vm);
    }

    [Authorize(Policy = "REPORTES.INDICADOR.VER")]
    public async Task<IActionResult> FinancialStatement(Guid periodId, string type)
    {
        var vm = type == "BALANCE"
            ? await _intelligenceService.GetBalanceGeneralContextAsync(_entidadProvider.CurrentEntidadId, periodId)
            : await _intelligenceService.GetEstadoResultadosContextAsync(_entidadProvider.CurrentEntidadId, periodId);

        return View("FinancialReport", vm);
    }

    [HttpGet]
    public async Task<IActionResult> ExportFinancialStatement(Guid periodId, string type)
    {
        var vm = type == "BALANCE"
            ? await _intelligenceService.GetBalanceGeneralContextAsync(_entidadProvider.CurrentEntidadId, periodId)
            : await _intelligenceService.GetEstadoResultadosContextAsync(_entidadProvider.CurrentEntidadId, periodId);

        var data = type == "BALANCE"
            ? vm.Balance.Activos.Concat(vm.Balance.Pasivos).Concat(vm.Balance.Patrimonio)
            : vm.Resultados;

        var csv = _exportService.ExportToCsv(data);
        return File(csv, "text/csv", $"Reporte_{type}_{vm.PeriodName.Replace("/", "_")}.csv");
    }
}
