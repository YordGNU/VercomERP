using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
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
    private readonly IReportingService _reportingService;
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IEntidadProvider _entidadProvider;

    public ReportsController(IIntelligenceService intelligenceService, IAccountingService accountingService,
        IExportService exportService, IReportingService reportingService,
        AppDbContext db, IWebHostEnvironment env, IEntidadProvider entidadProvider)
    {
        _intelligenceService = intelligenceService;
        _accountingService = accountingService;
        _exportService = exportService;
        _reportingService = reportingService;
        _db = db;
        _env = env;
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

    public class GeneratePackageRequest
    {
        public Guid EntidadId { get; set; }
        public Guid PeriodoId { get; set; }
    }

    [HttpPost("generate-package")]
    [Authorize(Policy = "REPORTES.INDICADOR.VER")]
    public async Task<IActionResult> GeneratePackage([FromBody] GeneratePackageRequest req)
    {
        if (req.EntidadId == Guid.Empty || req.PeriodoId == Guid.Empty)
            return BadRequest("EntidadId y PeriodoId son requeridos.");

        try
        {
            var packageId = await _reportingService.GenerateMonthlyPackageAsync(req.EntidadId, req.PeriodoId);
            var record = await _db.PaqueteInformacions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == packageId);
            return Ok(new { packageId, fileName = $"{packageId}.zip", record?.Estado });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generando paquete: {ex.Message}");
        }
    }

    [HttpGet("package/{id:guid}/download")]
    [Authorize(Policy = "REPORTES.INDICADOR.VER")]
    public async Task<IActionResult> DownloadPackage(Guid id)
    {
        var record = await _db.PaqueteInformacions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (record == null) return NotFound("Paquete no encontrado.");

        var filePath = Path.Combine(_env.WebRootPath ?? "", "uploads", "paquetes", $"{id}.zip");
        if (!System.IO.File.Exists(filePath)) return NotFound("Archivo no encontrado.");

        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(bytes, "application/zip", $"{id}.zip");
    }
}
