using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IBIService _biService;
    private readonly IAccountingService _accountingService;

    public DashboardController(IBIService biService, IAccountingService accountingService)
    {
        _biService = biService;
        _accountingService = accountingService;
    }

    public async Task<IActionResult> Index()
    {
        var entidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        var period = await _accountingService.GetOrCreateActivePeriodAsync(entidadId, DateTime.Now);

        var stats = await _biService.GetDashboardStatsAsync(entidadId, period?.Id ?? Guid.Empty);
        return View(stats);
    }
}
