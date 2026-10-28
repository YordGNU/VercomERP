using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VERCOM.Models;
using VERCOM.Security;
using VERCOM.Services;
using VERCOM.ViewModels;

namespace VERCOM.Controllers;

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

        vm.PeriodId = periodId;
        vm.ReportType = type;

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

    [HttpGet]
    public async Task<IActionResult> ExportFinancialStatementPdf(Guid periodId, string type)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var vm = type == "BALANCE"
            ? await _intelligenceService.GetBalanceGeneralContextAsync(_entidadProvider.CurrentEntidadId, periodId)
            : await _intelligenceService.GetEstadoResultadosContextAsync(_entidadProvider.CurrentEntidadId, periodId);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.Header().AlignCenter().Column(col =>
                {
                    col.Item().Text(vm.EntityName).FontSize(14).Bold();
                    col.Item().Text(vm.ReportName).FontSize(12).FontColor(Colors.Grey.Darken2);
                    col.Item().Text($"AL CIERRE DE: {vm.PeriodName}").FontSize(10).Italic();
                    col.Item().PaddingTop(4).LineHorizontal(1);
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    if (vm.Balance != null)
                    {
                        col.Item().Text("ACTIVOS").Bold().FontSize(11);
                        foreach (var a in vm.Balance.Activos)
                            col.Item().Row(r => { r.RelativeItem().Text($"{a.Codigo} - {a.Nombre}").FontSize(9); r.ConstantItem(90).AlignRight().Text(a.Saldo.ToString("N2")).FontSize(9); });
                        col.Item().Row(r => { r.RelativeItem().Text("TOTAL ACTIVOS").Bold().FontSize(9); r.ConstantItem(90).AlignRight().Text(vm.Balance.TotalActivos.ToString("N2")).Bold().FontSize(9); });
                        col.Item().PaddingTop(6);

                        col.Item().Text("PASIVOS").Bold().FontSize(11);
                        foreach (var p in vm.Balance.Pasivos)
                            col.Item().Row(r => { r.RelativeItem().Text($"{p.Codigo} - {p.Nombre}").FontSize(9); r.ConstantItem(90).AlignRight().Text(p.Saldo.ToString("N2")).FontSize(9); });
                        col.Item().Row(r => { r.RelativeItem().Text("TOTAL PASIVOS").Bold().FontSize(9); r.ConstantItem(90).AlignRight().Text(vm.Balance.TotalPasivos.ToString("N2")).Bold().FontSize(9); });
                        col.Item().PaddingTop(6);

                        col.Item().Text("PATRIMONIO").Bold().FontSize(11);
                        foreach (var p in vm.Balance.Patrimonio)
                            col.Item().Row(r => { r.RelativeItem().Text($"{p.Codigo} - {p.Nombre}").FontSize(9); r.ConstantItem(90).AlignRight().Text(p.Saldo.ToString("N2")).FontSize(9); });
                        col.Item().Row(r => { r.RelativeItem().Text("TOTAL PATRIMONIO").Bold().FontSize(9); r.ConstantItem(90).AlignRight().Text(vm.Balance.TotalPatrimonio.ToString("N2")).Bold().FontSize(9); });
                        col.Item().PaddingTop(8).LineHorizontal(1);
                        col.Item().Row(r => { r.RelativeItem().Text("DIFERENCIA (Activo - Pasivo - Patrimonio)").Bold().FontSize(9); r.ConstantItem(90).AlignRight().Text(vm.Balance.Diferencia.ToString("N2")).Bold().FontSize(9); });
                    }
                    else if (vm.Resultados != null)
                    {
                        col.Item().Text("ESTADO DE RESULTADOS").Bold().FontSize(11);
                        foreach (var r in vm.Resultados)
                            col.Item().Row(row => { row.RelativeItem().Text($"{r.Codigo} - {r.Nombre}").FontSize(9); row.ConstantItem(90).AlignRight().Text(r.Saldo.ToString("N2")).FontSize(9); });
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generado: ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(vm.GeneratedAt.ToString("dd/MM/yyyy HH:mm")).FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });

        using var ms = new MemoryStream();
        document.GeneratePdf(ms);
        return File(ms.ToArray(), "application/pdf", $"Reporte_{type}_{vm.PeriodName.Replace("/", "_")}.pdf");
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
