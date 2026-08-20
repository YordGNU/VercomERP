using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Controllers
{
    public class ActivoFijoDepreciacionController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public ActivoFijoDepreciacionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ActivoFijoDepreciacion
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ActivoFijoDepreciacions.Include(a => a.ActivoFijo).Include(a => a.Asiento).Include(a => a.Periodo);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ActivoFijoDepreciacion/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activoFijoDepreciacion = await _context.ActivoFijoDepreciacions
                .Include(a => a.ActivoFijo)
                .Include(a => a.Asiento)
                .Include(a => a.Periodo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (activoFijoDepreciacion == null)
            {
                return NotFound();
            }

            return View(activoFijoDepreciacion);
        }

        // GET: ActivoFijoDepreciacion/Create
        public IActionResult Create()
        {
            ViewData["ActivoFijoId"] = new SelectList(_context.ActivoFijos, "Id", "Id");
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id");
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id");
            return View();
        }

        // POST: ActivoFijoDepreciacion/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ActivoFijoId,PeriodoId,Monto,AsientoId,CalculadoEn")] ActivoFijoDepreciacion activoFijoDepreciacion)
        {
            if (ModelState.IsValid)
            {
                activoFijoDepreciacion.Id = Guid.NewGuid();
                _context.Add(activoFijoDepreciacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ActivoFijoId"] = new SelectList(_context.ActivoFijos, "Id", "Id", activoFijoDepreciacion.ActivoFijoId);
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id", activoFijoDepreciacion.AsientoId);
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id", activoFijoDepreciacion.PeriodoId);
            return View(activoFijoDepreciacion);
        }

        // GET: ActivoFijoDepreciacion/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activoFijoDepreciacion = await _context.ActivoFijoDepreciacions.FindAsync(id);
            if (activoFijoDepreciacion == null)
            {
                return NotFound();
            }
            ViewData["ActivoFijoId"] = new SelectList(_context.ActivoFijos, "Id", "Id", activoFijoDepreciacion.ActivoFijoId);
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id", activoFijoDepreciacion.AsientoId);
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id", activoFijoDepreciacion.PeriodoId);
            return View(activoFijoDepreciacion);
        }

        // POST: ActivoFijoDepreciacion/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ActivoFijoId,PeriodoId,Monto,AsientoId,CalculadoEn")] ActivoFijoDepreciacion activoFijoDepreciacion)
        {
            if (id != activoFijoDepreciacion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(activoFijoDepreciacion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActivoFijoDepreciacionExists(activoFijoDepreciacion.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ActivoFijoId"] = new SelectList(_context.ActivoFijos, "Id", "Id", activoFijoDepreciacion.ActivoFijoId);
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id", activoFijoDepreciacion.AsientoId);
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id", activoFijoDepreciacion.PeriodoId);
            return View(activoFijoDepreciacion);
        }

        // GET: ActivoFijoDepreciacion/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activoFijoDepreciacion = await _context.ActivoFijoDepreciacions
                .Include(a => a.ActivoFijo)
                .Include(a => a.Asiento)
                .Include(a => a.Periodo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (activoFijoDepreciacion == null)
            {
                return NotFound();
            }

            return View(activoFijoDepreciacion);
        }

        // POST: ActivoFijoDepreciacion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var activoFijoDepreciacion = await _context.ActivoFijoDepreciacions.FindAsync(id);
            if (activoFijoDepreciacion != null)
            {
                _context.ActivoFijoDepreciacions.Remove(activoFijoDepreciacion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActivoFijoDepreciacionExists(Guid id)
        {
            return _context.ActivoFijoDepreciacions.Any(e => e.Id == id);
        }
    }
}
