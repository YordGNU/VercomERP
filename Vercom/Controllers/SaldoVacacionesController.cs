using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Security;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class SaldoVacacionesController : Controller
{
    private readonly IHRService _hrService;
    private readonly IHRReportService _hrReportService;
    private readonly IEntidadProvider _entidadProvider;

    public SaldoVacacionesController(IHRService hrService, IHRReportService hrReportService, IEntidadProvider entidadProvider)
    {
        _hrService = hrService;
        _hrReportService = hrReportService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "RRHH.REPORTE.VER")]
    public async Task<IActionResult> Index()
    {
        var entidadId = _entidadProvider.CurrentEntidadId;
        var saldos = await _hrReportService.GetVacationSubledgerAsync(entidadId);
        return View(saldos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.EMPLEADO.EDITAR")]
    public async Task<IActionResult> RecordDisfrute(Guid id, decimal dias, string? observaciones)
    {
        var result = await _hrService.RecordVacationEnjoymentAsync(id, dias, observaciones);

        return Json(new { success = result.Succeeded, message = result.Message });
    }
}
