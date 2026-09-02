using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Security;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IIntelligenceService _intelligenceService;
    private readonly IAdminService _adminService;
    private readonly IEntidadProvider _entidadProvider;

    public DashboardController(IIntelligenceService intelligenceService, IAdminService adminService, IEntidadProvider entidadProvider)
    {
        _intelligenceService = intelligenceService;
        _adminService = adminService;
        _entidadProvider = entidadProvider;
    }

    public async Task<IActionResult> Index()
    {
        if (_entidadProvider.IsMaster)
        {
            var masterVm = await _adminService.GetMasterDashboardStatsAsync();
            return View("Master", masterVm);
        }

        var vm = await _intelligenceService.GetDashboardContextAsync(_entidadProvider.CurrentEntidadId);
        return View(vm);
    }
}
