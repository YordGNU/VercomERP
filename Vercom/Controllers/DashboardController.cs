using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Vercom.Security;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly IIntelligenceService _intelligenceService;
    private readonly IAdminService _adminService;
    private readonly IEntidadProvider _entidadProvider;
    private readonly IMemoryCache _cache;

    public DashboardController(IIntelligenceService intelligenceService, IAdminService adminService, IEntidadProvider entidadProvider, IMemoryCache cache)
    {
        _intelligenceService = intelligenceService;
        _adminService = adminService;
        _entidadProvider = entidadProvider;
        _cache = cache;
    }

    public async Task<IActionResult> Index()
    {
        if (_entidadProvider.IsMaster)
        {
            var masterVm = await _cache.GetOrCreateAsync("dashboard:master", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                return _adminService.GetMasterDashboardStatsAsync();
            });
            return View("Master", masterVm);
        }

        var entidadId = _entidadProvider.CurrentEntidadId;
        var vm = await _cache.GetOrCreateAsync($"dashboard:entidad:{entidadId}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return _intelligenceService.GetDashboardContextAsync(entidadId);
        });
        return View(vm);
    }
}
