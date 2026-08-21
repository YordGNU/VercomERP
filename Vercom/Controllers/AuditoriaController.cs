using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize(Policy = "SEGURIDAD.AUDITORIA.VER")]
public class AuditoriaController : Controller
{
    private readonly IAdminService _adminService;

    public AuditoriaController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index(string? filterUser, string? filterTable, DateTime? start, DateTime? end)
    {
        var vm = await _adminService.GetAuditLogsAsync(filterUser, filterTable, start, end);
        return View(vm);
    }

    public async Task<IActionResult> Details(long id)
    {
        var entry = await _adminService.GetAuditDetailAsync(id);
        if (entry == null) return NotFound();
        return View(entry);
    }
}
