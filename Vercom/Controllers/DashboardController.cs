using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Security;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IIntelligenceService _intelligenceService;
    private readonly IEntidadProvider _entidadProvider;

    public DashboardController(IIntelligenceService intelligenceService, IEntidadProvider entidadProvider)
    {
        _intelligenceService = intelligenceService;
        _entidadProvider = entidadProvider;
    }

    public async Task<IActionResult> Index()
    {
        var vm = await _intelligenceService.GetDashboardContextAsync(_entidadProvider.CurrentEntidadId);
        return View(vm);
    }
}
