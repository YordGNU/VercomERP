using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers
{
    [Authorize]
    public class AsientoContableController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAccountingService _accountingService;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public AsientoContableController(AppDbContext context, IAccountingService accountingService)
        {
            _context = context;
            _accountingService = accountingService;
        }

        // GET: AsientoContable
        [Authorize(Policy = "CONTABILIDAD.ASIENTO.VER")]
        public async Task<IActionResult> Index(Guid? periodId)
        {
            if (periodId == null)
            {
                var currentPeriod = await _accountingService.GetOrCreateActivePeriodAsync(CurrentEntidadId, DateTime.Now);
                periodId = currentPeriod?.Id;
            }

            var entries = await _accountingService.GetEntriesByPeriodAsync(periodId ?? Guid.Empty);

            ViewBag.Periods = new SelectList(_context.PeriodoContables
                .Where(p => p.EntidadId == CurrentEntidadId)
                .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes), "Id", "Mes", periodId);

            return View(entries);
        }

        // GET: AsientoContable/Details/5
        [Authorize(Policy = "CONTABILIDAD.ASIENTO.VER")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var asientoContable = await _context.AsientoContables
                .Include(a => a.AsientoReversion)
                .Include(a => a.Periodo)
                .Include(a => a.TipoComprobante)
                .Include(a => a.AsientoDetalles)
                    .ThenInclude(d => d.Cuenta)
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (asientoContable == null) return NotFound();

            return View(asientoContable);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CONTABILIDAD.ASIENTO.CREAR")]
        public async Task<IActionResult> Post(Guid id)
        {
            var result = await _accountingService.PostEntryAsync(id);
            if (result.Succeeded) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CONTABILIDAD.ASIENTO.REVERTIR")]
        public async Task<IActionResult> Reverse(Guid id, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                TempData["Error"] = "Debe proporcionar un motivo para la reversión.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var result = await _accountingService.ReverseEntryAsync(id, reason);
            if (result.Succeeded) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        // GET: AsientoContable/Create
        [Authorize(Policy = "CONTABILIDAD.ASIENTO.CREAR")]
        public IActionResult Create()
        {
            ViewData["TipoComprobanteId"] = new SelectList(_context.TipoComprobantes, "Id", "Nombre");
            ViewData["Cuentas"] = _context.CuentaContables
                .Where(c => c.EntidadId == CurrentEntidadId && c.AceptaMovimiento && c.Activo)
                .OrderBy(c => c.Codigo)
                .Select(c => new { c.Id, Display = c.Codigo + " " + c.Nombre })
                .ToList();

            return View(new AsientoContable { Fecha = DateOnly.FromDateTime(DateTime.Now), Estado = "BORRADOR" });
        }

        // POST: AsientoContable/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CONTABILIDAD.ASIENTO.CREAR")]
        public async Task<IActionResult> Create(AsientoContable asientoContable)
        {
            if (ModelState.IsValid)
            {
                asientoContable.EntidadId = CurrentEntidadId;
                asientoContable.CreadoPor = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

                var result = await _accountingService.CreateEntryAsync(asientoContable);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Details), new { id = result.Entry?.Id });
                }
                ModelState.AddModelError("", result.Message);
            }

            ViewData["TipoComprobanteId"] = new SelectList(_context.TipoComprobantes, "Id", "Nombre", asientoContable.TipoComprobanteId);
            return View(asientoContable);
        }

        // GET: AsientoContable/Delete/5
        [Authorize(Policy = "CONTABILIDAD.ASIENTO.CREAR")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var asientoContable = await _context.AsientoContables
                .Include(a => a.Periodo)
                .Include(a => a.TipoComprobante)
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (asientoContable == null) return NotFound();
            if (asientoContable.Estado == "CONTABILIZADO")
            {
                TempData["Error"] = "No se puede eliminar un asiento ya contabilizado.";
                return RedirectToAction(nameof(Index));
            }

            return View(asientoContable);
        }

        // POST: AsientoContable/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CONTABILIDAD.ASIENTO.CREAR")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var asientoContable = await _context.AsientoContables
                .FirstOrDefaultAsync(a => a.Id == id && a.EntidadId == CurrentEntidadId);

            if (asientoContable != null && asientoContable.Estado != "CONTABILIZADO")
            {
                _context.AsientoContables.Remove(asientoContable);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AsientoContableExists(Guid id)
        {
            return _context.AsientoContables.Any(e => e.Id == id && e.EntidadId == CurrentEntidadId);
        }
    }
}
