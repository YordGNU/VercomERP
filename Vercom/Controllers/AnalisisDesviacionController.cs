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
    public class AnalisisDesviacionController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public AnalisisDesviacionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AnalisisDesviacion
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.AnalisisDesviacions.Include(a => a.OrdenProduccion);
            return View(await appDbContext.ToListAsync());
        }

        // GET: AnalisisDesviacion/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analisisDesviacion = await _context.AnalisisDesviacions
                .Include(a => a.OrdenProduccion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (analisisDesviacion == null)
            {
                return NotFound();
            }

            return View(analisisDesviacion);
        }

        // GET: AnalisisDesviacion/Create
        public IActionResult Create()
        {
            ViewData["OrdenProduccionId"] = new SelectList(_context.OrdenProduccions, "Id", "Id");
            return View();
        }

        // POST: AnalisisDesviacion/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrdenProduccionId,Componente,CostoEstandar,CostoReal,Desviacion,AnalizadoEn")] AnalisisDesviacion analisisDesviacion)
        {
            if (ModelState.IsValid)
            {
                analisisDesviacion.Id = Guid.NewGuid();
                _context.Add(analisisDesviacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrdenProduccionId"] = new SelectList(_context.OrdenProduccions, "Id", "Id", analisisDesviacion.OrdenProduccionId);
            return View(analisisDesviacion);
        }

        // GET: AnalisisDesviacion/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analisisDesviacion = await _context.AnalisisDesviacions.FindAsync(id);
            if (analisisDesviacion == null)
            {
                return NotFound();
            }
            ViewData["OrdenProduccionId"] = new SelectList(_context.OrdenProduccions, "Id", "Id", analisisDesviacion.OrdenProduccionId);
            return View(analisisDesviacion);
        }

        // POST: AnalisisDesviacion/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,OrdenProduccionId,Componente,CostoEstandar,CostoReal,Desviacion,AnalizadoEn")] AnalisisDesviacion analisisDesviacion)
        {
            if (id != analisisDesviacion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(analisisDesviacion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnalisisDesviacionExists(analisisDesviacion.Id))
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
            ViewData["OrdenProduccionId"] = new SelectList(_context.OrdenProduccions, "Id", "Id", analisisDesviacion.OrdenProduccionId);
            return View(analisisDesviacion);
        }

        // GET: AnalisisDesviacion/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analisisDesviacion = await _context.AnalisisDesviacions
                .Include(a => a.OrdenProduccion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (analisisDesviacion == null)
            {
                return NotFound();
            }

            return View(analisisDesviacion);
        }

        // POST: AnalisisDesviacion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var analisisDesviacion = await _context.AnalisisDesviacions.FindAsync(id);
            if (analisisDesviacion != null)
            {
                _context.AnalisisDesviacions.Remove(analisisDesviacion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnalisisDesviacionExists(Guid id)
        {
            return _context.AnalisisDesviacions.Any(e => e.Id == id);
        }
    }
}
