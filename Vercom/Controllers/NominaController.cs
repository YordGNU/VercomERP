using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.Security;

namespace Vercom.Controllers;

[Authorize]
public class NominaController : Controller
{
    private readonly IPayrollService _payrollService;
    private readonly IEntidadProvider _entidadProvider;

    public NominaController(IPayrollService payrollService, IEntidadProvider entidadProvider)
    {
        _payrollService = payrollService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "RRHH.NOMINA.VER")]
    public async Task<IActionResult> Index()
    {
        var vm = await _payrollService.GetPayrollIndexContextAsync();
        return View(vm);
    }

    [Authorize(Policy = "RRHH.NOMINA.VER")]
    public async Task<IActionResult> Details(Guid id)
    {
        var vm = await _payrollService.GetPayrollDetailsContextAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.NOMINA.CALCULAR")]
    public async Task<IActionResult> Calculate(short anio, short mes)
    {
        var result = await _payrollService.CalculatePayrollAsync(_entidadProvider.CurrentEntidadId, anio, mes);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.NOMINA.APROBAR")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _payrollService.ApprovePayrollAsync(id, _entidadProvider.CurrentUsuarioId);
        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Details), new { id });
    }
}
