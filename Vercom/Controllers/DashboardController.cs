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

    public async Task<IActionResult> Index(string rango = "mes")
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

        var rangoNorm = (rango ?? "mes").Trim().ToLowerInvariant();
        if (rangoNorm != "hoy" && rangoNorm != "7d" && rangoNorm != "30d"
            && rangoNorm != "mes" && rangoNorm != "trimestre")
        {
            rangoNorm = "mes";
        }

        var entidadId = _entidadProvider.CurrentEntidadId;
        var cacheKey = $"dashboard:entidad:{entidadId}:{rangoNorm}";
        var vm = await _cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return _intelligenceService.GetDashboardContextAsync(entidadId, rangoNorm);
        });
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> PosPartial()
    {
        if (_entidadProvider.IsMaster)
        {
            return PartialView("_PosCard", new Vercom.ViewModels.DashboardViewModel());
        }

        var entidadId = _entidadProvider.CurrentEntidadId;
        var vm = await _intelligenceService.GetPosContextAsync(entidadId);
        return PartialView("_PosCard", vm);
    }
}
