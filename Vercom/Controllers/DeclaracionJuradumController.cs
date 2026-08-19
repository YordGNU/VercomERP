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
    public class DeclaracionJuradumController : Controller
    {
        private readonly AppDbContext _context;

        public DeclaracionJuradumController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DeclaracionJuradum
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.DeclaracionJurada.Include(d => d.Asiento).Include(d => d.Periodo).Include(d => d.TipoObligacion);
            return View(await appDbContext.ToListAsync());
        }

        // GET: DeclaracionJuradum/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var declaracionJuradum = await _context.DeclaracionJurada
                .Include(d => d.Asiento)
                .Include(d => d.Periodo)
                .Include(d => d.TipoObligacion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (declaracionJuradum == null)
            {
                return NotFound();
            }

            return View(declaracionJuradum);
        }

        // GET: DeclaracionJuradum/Create
        public IActionResult Create()
        {
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id");
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id");
            ViewData["TipoObligacionId"] = new SelectList(_context.TipoObligacionFiscals, "Id", "Id");
            return View();
        }

        // POST: DeclaracionJuradum/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,TipoObligacionId,PeriodoId,BaseImponible,MontoCalculado,MontoPagado,FechaLimite,FechaPresentacion,Estado,NumeroDj,AsientoId,GeneradoPor,CreadoEn")] DeclaracionJuradum declaracionJuradum)
        {
            if (ModelState.IsValid)
            {
                declaracionJuradum.Id = Guid.NewGuid();
                _context.Add(declaracionJuradum);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id", declaracionJuradum.AsientoId);
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id", declaracionJuradum.PeriodoId);
            ViewData["TipoObligacionId"] = new SelectList(_context.TipoObligacionFiscals, "Id", "Id", declaracionJuradum.TipoObligacionId);
            return View(declaracionJuradum);
        }

        // GET: DeclaracionJuradum/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var declaracionJuradum = await _context.DeclaracionJurada.FindAsync(id);
            if (declaracionJuradum == null)
            {
                return NotFound();
            }
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id", declaracionJuradum.AsientoId);
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id", declaracionJuradum.PeriodoId);
            ViewData["TipoObligacionId"] = new SelectList(_context.TipoObligacionFiscals, "Id", "Id", declaracionJuradum.TipoObligacionId);
            return View(declaracionJuradum);
        }

        // POST: DeclaracionJuradum/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,TipoObligacionId,PeriodoId,BaseImponible,MontoCalculado,MontoPagado,FechaLimite,FechaPresentacion,Estado,NumeroDj,AsientoId,GeneradoPor,CreadoEn")] DeclaracionJuradum declaracionJuradum)
        {
            if (id != declaracionJuradum.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(declaracionJuradum);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeclaracionJuradumExists(declaracionJuradum.Id))
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
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id", declaracionJuradum.AsientoId);
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id", declaracionJuradum.PeriodoId);
            ViewData["TipoObligacionId"] = new SelectList(_context.TipoObligacionFiscals, "Id", "Id", declaracionJuradum.TipoObligacionId);
            return View(declaracionJuradum);
        }

        // GET: DeclaracionJuradum/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var declaracionJuradum = await _context.DeclaracionJurada
                .Include(d => d.Asiento)
                .Include(d => d.Periodo)
                .Include(d => d.TipoObligacion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (declaracionJuradum == null)
            {
                return NotFound();
            }

            return View(declaracionJuradum);
        }

        // POST: DeclaracionJuradum/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var declaracionJuradum = await _context.DeclaracionJurada.FindAsync(id);
            if (declaracionJuradum != null)
            {
                _context.DeclaracionJurada.Remove(declaracionJuradum);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeclaracionJuradumExists(Guid id)
        {
            return _context.DeclaracionJurada.Any(e => e.Id == id);
        }
    }
}
