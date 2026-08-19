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
    public class PosRangoNumeracionController : Controller
    {
        private readonly AppDbContext _context;

        public PosRangoNumeracionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PosRangoNumeracion
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PosRangoNumeracions.Include(p => p.DispositivoPos);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PosRangoNumeracion/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posRangoNumeracion = await _context.PosRangoNumeracions
                .Include(p => p.DispositivoPos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (posRangoNumeracion == null)
            {
                return NotFound();
            }

            return View(posRangoNumeracion);
        }

        // GET: PosRangoNumeracion/Create
        public IActionResult Create()
        {
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id");
            return View();
        }

        // POST: PosRangoNumeracion/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DispositivoPosId,TipoDocumento,Serie,NumeroDesde,NumeroHasta,NumeroSiguienteLocal,AsignadoEn,Agotado")] PosRangoNumeracion posRangoNumeracion)
        {
            if (ModelState.IsValid)
            {
                posRangoNumeracion.Id = Guid.NewGuid();
                _context.Add(posRangoNumeracion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id", posRangoNumeracion.DispositivoPosId);
            return View(posRangoNumeracion);
        }

        // GET: PosRangoNumeracion/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posRangoNumeracion = await _context.PosRangoNumeracions.FindAsync(id);
            if (posRangoNumeracion == null)
            {
                return NotFound();
            }
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id", posRangoNumeracion.DispositivoPosId);
            return View(posRangoNumeracion);
        }

        // POST: PosRangoNumeracion/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,DispositivoPosId,TipoDocumento,Serie,NumeroDesde,NumeroHasta,NumeroSiguienteLocal,AsignadoEn,Agotado")] PosRangoNumeracion posRangoNumeracion)
        {
            if (id != posRangoNumeracion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(posRangoNumeracion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PosRangoNumeracionExists(posRangoNumeracion.Id))
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
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id", posRangoNumeracion.DispositivoPosId);
            return View(posRangoNumeracion);
        }

        // GET: PosRangoNumeracion/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posRangoNumeracion = await _context.PosRangoNumeracions
                .Include(p => p.DispositivoPos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (posRangoNumeracion == null)
            {
                return NotFound();
            }

            return View(posRangoNumeracion);
        }

        // POST: PosRangoNumeracion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var posRangoNumeracion = await _context.PosRangoNumeracions.FindAsync(id);
            if (posRangoNumeracion != null)
            {
                _context.PosRangoNumeracions.Remove(posRangoNumeracion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PosRangoNumeracionExists(Guid id)
        {
            return _context.PosRangoNumeracions.Any(e => e.Id == id);
        }
    }
}
