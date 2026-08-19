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
    public class PeriodoContableController : Controller
    {
        private readonly AppDbContext _context;

        public PeriodoContableController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PeriodoContable
        public async Task<IActionResult> Index()
        {
            return View(await _context.PeriodoContables.ToListAsync());
        }

        // GET: PeriodoContable/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var periodoContable = await _context.PeriodoContables
                .FirstOrDefaultAsync(m => m.Id == id);
            if (periodoContable == null)
            {
                return NotFound();
            }

            return View(periodoContable);
        }

        // GET: PeriodoContable/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PeriodoContable/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,Anio,Mes,FechaInicio,FechaFin,Estado,CerradoPor,CerradoEn")] PeriodoContable periodoContable)
        {
            if (ModelState.IsValid)
            {
                periodoContable.Id = Guid.NewGuid();
                _context.Add(periodoContable);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(periodoContable);
        }

        // GET: PeriodoContable/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var periodoContable = await _context.PeriodoContables.FindAsync(id);
            if (periodoContable == null)
            {
                return NotFound();
            }
            return View(periodoContable);
        }

        // POST: PeriodoContable/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,Anio,Mes,FechaInicio,FechaFin,Estado,CerradoPor,CerradoEn")] PeriodoContable periodoContable)
        {
            if (id != periodoContable.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(periodoContable);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PeriodoContableExists(periodoContable.Id))
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
            return View(periodoContable);
        }

        // GET: PeriodoContable/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var periodoContable = await _context.PeriodoContables
                .FirstOrDefaultAsync(m => m.Id == id);
            if (periodoContable == null)
            {
                return NotFound();
            }

            return View(periodoContable);
        }

        // POST: PeriodoContable/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var periodoContable = await _context.PeriodoContables.FindAsync(id);
            if (periodoContable != null)
            {
                _context.PeriodoContables.Remove(periodoContable);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PeriodoContableExists(Guid id)
        {
            return _context.PeriodoContables.Any(e => e.Id == id);
        }
    }
}
