using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers
{
    [Authorize]
    public class NominaController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPayrollService _payrollService;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public NominaController(AppDbContext context, IPayrollService payrollService)
        {
            _context = context;
            _payrollService = payrollService;
        }

        [Authorize(Policy = "RRHH.NOMINA.VER")]
        public async Task<IActionResult> Index()
        {
            var periodos = await _context.PeriodoNominas
                .Where(p => p.EntidadId == CurrentEntidadId)
                .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes)
                .ToListAsync();
            return View(periodos);
        }

        [Authorize(Policy = "RRHH.NOMINA.VER")]
        public async Task<IActionResult> Details(Guid id)
        {
            var periodo = await _context.PeriodoNominas
                .Include(p => p.NominaDetalles).ThenInclude(d => d.Empleado)
                .FirstOrDefaultAsync(p => p.Id == id && p.EntidadId == CurrentEntidadId);

            if (periodo == null) return NotFound();

            return View(periodo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RRHH.NOMINA.CALCULAR")]
        public async Task<IActionResult> Calculate(short anio, short mes)
        {
            var result = await _payrollService.CalculatePayrollAsync(CurrentEntidadId, anio, mes);
            if (result.Succeeded) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RRHH.NOMINA.APROBAR")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _payrollService.ApprovePayrollAsync(id, userId);

            if (result.Succeeded) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
