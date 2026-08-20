using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Controllers
{
    [Authorize]
    public class CuentaContableController : Controller
    {
        private readonly AppDbContext _context;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public CuentaContableController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CuentaContable
        [Authorize(Policy = "CONTABILIDAD.CUENTA.VER")]
        public async Task<IActionResult> Index()
        {
            var cuentas = await _context.CuentaContables
                .Where(c => c.EntidadId == CurrentEntidadId)
                .OrderBy(c => c.Codigo)
                .ToListAsync();
            return View(cuentas);
        }

        // GET: CuentaContable/Details/5
        [Authorize(Policy = "CONTABILIDAD.CUENTA.VER")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var cuentaContable = await _context.CuentaContables
                .Include(c => c.AsientoDetalles)
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (cuentaContable == null) return NotFound();

            return View(cuentaContable);
        }

        // GET: CuentaContable/Create
        [Authorize(Policy = "CONTABILIDAD.CUENTA.CREAR")]
        public IActionResult Create()
        {
            PrepareViewBags();
            return View(new CuentaContable { Activo = true, Moneda = "CUP", Nivel = 1 });
        }

        // POST: CuentaContable/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CONTABILIDAD.CUENTA.CREAR")]
        public async Task<IActionResult> Create(CuentaContable cuentaContable)
        {
            if (ModelState.IsValid)
            {
                cuentaContable.Id = Guid.NewGuid();
                cuentaContable.EntidadId = CurrentEntidadId;
                cuentaContable.CreadoEn = DateTimeOffset.Now;

                _context.Add(cuentaContable);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PrepareViewBags(cuentaContable.CuentaPadreId);
            return View(cuentaContable);
        }

        // GET: CuentaContable/Edit/5
        [Authorize(Policy = "CONTABILIDAD.CUENTA.EDITAR")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var cuentaContable = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Id == id && c.EntidadId == CurrentEntidadId);
            if (cuentaContable == null) return NotFound();

            PrepareViewBags(cuentaContable.CuentaPadreId);
            return View(cuentaContable);
        }

        // POST: CuentaContable/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CONTABILIDAD.CUENTA.EDITAR")]
        public async Task<IActionResult> Edit(Guid id, CuentaContable cuentaContable)
        {
            if (id != cuentaContable.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    cuentaContable.EntidadId = CurrentEntidadId;
                    _context.Update(cuentaContable);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuentaContableExists(cuentaContable.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PrepareViewBags(cuentaContable.CuentaPadreId);
            return View(cuentaContable);
        }

        // GET: CuentaContable/Delete/5
        [Authorize(Policy = "CONTABILIDAD.CUENTA.ELIMINAR")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var cuentaContable = await _context.CuentaContables
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (cuentaContable == null) return NotFound();

            // Validar si tiene movimientos
            var hasMovements = await _context.AsientoDetalles.AnyAsync(d => d.CuentaId == id);
            if (hasMovements)
            {
                TempData["Error"] = "No se puede eliminar una cuenta que ya tiene movimientos contables.";
                return RedirectToAction(nameof(Index));
            }

            return View(cuentaContable);
        }

        // POST: CuentaContable/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CONTABILIDAD.CUENTA.ELIMINAR")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var cuentaContable = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Id == id && c.EntidadId == CurrentEntidadId);
            if (cuentaContable != null)
            {
                _context.CuentaContables.Remove(cuentaContable);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private void PrepareViewBags(Guid? selectedPadre = null)
        {
            var cuentasPadre = _context.CuentaContables
                .Where(c => c.EntidadId == CurrentEntidadId && !c.AceptaMovimiento)
                .OrderBy(c => c.Codigo)
                .Select(c => new { c.Id, Display = c.Codigo + " - " + c.Nombre })
                .ToList();

            ViewBag.CuentaPadreId = new SelectList(cuentasPadre, "Id", "Display", selectedPadre);

            ViewBag.Clases = new SelectList(new[] { "ACTIVO", "PASIVO", "PATRIMONIO", "INGRESOS", "GASTOS" });
            ViewBag.Naturalezas = new SelectList(new[] { "DEUDORA", "ACREEDORA" });
        }

        private bool CuentaContableExists(Guid id)
        {
            return _context.CuentaContables.Any(e => e.Id == id && e.EntidadId == CurrentEntidadId);
        }
    }
}
