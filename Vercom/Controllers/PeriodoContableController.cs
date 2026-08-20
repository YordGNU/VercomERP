using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers
{
    [Authorize]
    public class PeriodoContableController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAccountingService _accountingService;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public PeriodoContableController(AppDbContext context, IAccountingService accountingService)
        {
            _context = context;
            _accountingService = accountingService;
        }

        // GET: PeriodoContable
        [Authorize(Policy = "ACC_VIEW_PLAN")] // Reutilizando permiso de vista contable
        public async Task<IActionResult> Index()
        {
            var periodos = await _context.PeriodoContables
                .Where(p => p.EntidadId == CurrentEntidadId)
                .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes)
                .ToListAsync();
            return View(periodos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ACC_CLOSE_PERIOD")]
        public async Task<IActionResult> Close(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _accountingService.ClosePeriodAsync(id, userId);

            if (result.Succeeded) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        private bool PeriodoContableExists(Guid id)
        {
            return _context.PeriodoContables.Any(e => e.Id == id && e.EntidadId == CurrentEntidadId);
        }
    }
}
