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
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        private readonly IAccountingService _accountingService;

        public AsientoContableController(AppDbContext context, IAccountingService accountingService)
        {
            _context = context;
            _accountingService = accountingService;
        }

        // GET: AsientoContable
        public async Task<IActionResult> Index(Guid? periodId)
        {
            var entidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

            if (periodId == null)
            {
                // Por defecto, mostrar el periodo actual
                var currentPeriod = await _accountingService.GetOrCreateActivePeriodAsync(entidadId, DateTime.Now);
                periodId = currentPeriod?.Id;
            }

            var entries = await _accountingService.GetEntriesByPeriodAsync(periodId ?? Guid.Empty);
            ViewBag.Periods = new SelectList(_context.PeriodoContables
                .Where(p => p.EntidadId == entidadId)
                .OrderByDescending(p => p.Anio)
                .ThenByDescending(p => p.Mes), "Id", "Mes", periodId);
            return View(entries);
        }

        // GET: AsientoContable/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var asientoContable = await _context.AsientoContables
                .Include(a => a.AsientoReversion)
                .Include(a => a.Periodo)
                .Include(a => a.TipoComprobante)
                .Include(a => a.AsientoDetalles)
                    .ThenInclude(d => d.Cuenta)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (asientoContable == null) return NotFound();

            return View(asientoContable);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Guid id)
        {
            var result = await _accountingService.PostEntryAsync(id);
            if (result.Succeeded) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Reverse(Guid id, string reason)
        {
            var result = await _accountingService.ReverseEntryAsync(id, reason);
            if (result.Succeeded) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        // GET: AsientoContable/Create
        public IActionResult Create()
        {
            ViewData["TipoComprobanteId"] = new SelectList(_context.TipoComprobantes, "Id", "Nombre");
            ViewData["CuentaId"] = new SelectList(_context.CuentaContables.Where(c => c.AceptaMovimiento), "Id", "Nombre");
            return View(new AsientoContable { Fecha = DateOnly.FromDateTime(DateTime.Now) });
        }

        // POST: AsientoContable/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AsientoContable asientoContable)
        {
            if (ModelState.IsValid)
            {
                asientoContable.Id = Guid.NewGuid();
                asientoContable.EntidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
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

        // GET: AsientoContable/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var asientoContable = await _context.AsientoContables
                .Include(a => a.AsientoDetalles)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asientoContable == null) return NotFound();

            if (asientoContable.Estado == "CONTABILIZADO")
            {
                TempData["Error"] = "No se puede editar un asiento ya contabilizado.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewData["TipoComprobanteId"] = new SelectList(_context.TipoComprobantes, "Id", "Nombre", asientoContable.TipoComprobanteId);
            return View(asientoContable);
        }

        // POST: AsientoContable/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AsientoContable asientoContable)
        {
            if (id != asientoContable.Id) return NotFound();

            var existing = await _context.AsientoContables.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (existing?.Estado == "CONTABILIZADO")
            {
                return BadRequest("Inmutable.");
            }

            if (ModelState.IsValid)
            {
                // Implementar lógica de actualización en el servicio si es necesario
                _context.Update(asientoContable);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(asientoContable);
        }

        // GET: AsientoContable/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asientoContable = await _context.AsientoContables
                .Include(a => a.AsientoReversion)
                .Include(a => a.Periodo)
                .Include(a => a.TipoComprobante)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asientoContable == null)
            {
                return NotFound();
            }

            return View(asientoContable);
        }

        // POST: AsientoContable/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var asientoContable = await _context.AsientoContables.FindAsync(id);
            if (asientoContable != null)
            {
                _context.AsientoContables.Remove(asientoContable);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AsientoContableExists(Guid id)
        {
            return _context.AsientoContables.Any(e => e.Id == id);
        }
    }
}
