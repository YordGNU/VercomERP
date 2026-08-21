using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.Security;

namespace Vercom.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IIntelligenceService _intelligenceService;
    private readonly IEntidadProvider _entidadProvider;

    public ReportsController(IIntelligenceService intelligenceService, IEntidadProvider entidadProvider)
    {
        _intelligenceService = intelligenceService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "REPORTES.INDICADOR.VER")]
    public async Task<IActionResult> Index()
    {
        return View();
    }

    [Authorize(Policy = "REPORTES.INDICADOR.VER")]
    public async Task<IActionResult> FinancialStatement(Guid periodId, string type)
    {
        var vm = type == "BALANCE"
            ? await _intelligenceService.GetBalanceGeneralContextAsync(_entidadProvider.CurrentEntidadId, periodId)
            : await _intelligenceService.GetEstadoResultadosContextAsync(_entidadProvider.CurrentEntidadId, periodId);

        return View("FinancialReport", vm);
    }
}
