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
    public class PaqueteInformacionController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public PaqueteInformacionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PaqueteInformacion
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PaqueteInformacions.Include(p => p.Periodo);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PaqueteInformacion/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paqueteInformacion = await _context.PaqueteInformacions
                .Include(p => p.Periodo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paqueteInformacion == null)
            {
                return NotFound();
            }

            return View(paqueteInformacion);
        }

        // GET: PaqueteInformacion/Create
        public IActionResult Create()
        {
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id");
            return View();
        }

        // POST: PaqueteInformacion/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,PeriodoId,Tipo,Estado,GeneradoPor,GeneradoEn")] PaqueteInformacion paqueteInformacion)
        {
            if (ModelState.IsValid)
            {
                paqueteInformacion.Id = Guid.NewGuid();
                _context.Add(paqueteInformacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id", paqueteInformacion.PeriodoId);
            return View(paqueteInformacion);
        }

        // GET: PaqueteInformacion/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paqueteInformacion = await _context.PaqueteInformacions.FindAsync(id);
            if (paqueteInformacion == null)
            {
                return NotFound();
            }
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id", paqueteInformacion.PeriodoId);
            return View(paqueteInformacion);
        }

        // POST: PaqueteInformacion/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,PeriodoId,Tipo,Estado,GeneradoPor,GeneradoEn")] PaqueteInformacion paqueteInformacion)
        {
            if (id != paqueteInformacion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paqueteInformacion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaqueteInformacionExists(paqueteInformacion.Id))
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
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id", paqueteInformacion.PeriodoId);
            return View(paqueteInformacion);
        }

        // GET: PaqueteInformacion/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paqueteInformacion = await _context.PaqueteInformacions
                .Include(p => p.Periodo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paqueteInformacion == null)
            {
                return NotFound();
            }

            return View(paqueteInformacion);
        }

        // POST: PaqueteInformacion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var paqueteInformacion = await _context.PaqueteInformacions.FindAsync(id);
            if (paqueteInformacion != null)
            {
                _context.PaqueteInformacions.Remove(paqueteInformacion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaqueteInformacionExists(Guid id)
        {
            return _context.PaqueteInformacions.Any(e => e.Id == id);
        }
    }
}
