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
    public class SaldoVacacioneController : Controller
    {
        private readonly AppDbContext _context;

        public SaldoVacacioneController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SaldoVacacione
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.SaldoVacaciones.Include(s => s.Empleado);
            return View(await appDbContext.ToListAsync());
        }

        // GET: SaldoVacacione/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var saldoVacacione = await _context.SaldoVacaciones
                .Include(s => s.Empleado)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (saldoVacacione == null)
            {
                return NotFound();
            }

            return View(saldoVacacione);
        }

        // GET: SaldoVacacione/Create
        public IActionResult Create()
        {
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id");
            return View();
        }

        // POST: SaldoVacacione/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpleadoId,Anio,DiasAcumulados,DiasDisfrutados,DiasCompensados,SaldoActual")] SaldoVacacione saldoVacacione)
        {
            if (ModelState.IsValid)
            {
                saldoVacacione.Id = Guid.NewGuid();
                _context.Add(saldoVacacione);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", saldoVacacione.EmpleadoId);
            return View(saldoVacacione);
        }

        // GET: SaldoVacacione/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var saldoVacacione = await _context.SaldoVacaciones.FindAsync(id);
            if (saldoVacacione == null)
            {
                return NotFound();
            }
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", saldoVacacione.EmpleadoId);
            return View(saldoVacacione);
        }

        // POST: SaldoVacacione/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EmpleadoId,Anio,DiasAcumulados,DiasDisfrutados,DiasCompensados,SaldoActual")] SaldoVacacione saldoVacacione)
        {
            if (id != saldoVacacione.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(saldoVacacione);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SaldoVacacioneExists(saldoVacacione.Id))
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
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", saldoVacacione.EmpleadoId);
            return View(saldoVacacione);
        }

        // GET: SaldoVacacione/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var saldoVacacione = await _context.SaldoVacaciones
                .Include(s => s.Empleado)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (saldoVacacione == null)
            {
                return NotFound();
            }

            return View(saldoVacacione);
        }

        // POST: SaldoVacacione/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var saldoVacacione = await _context.SaldoVacaciones.FindAsync(id);
            if (saldoVacacione != null)
            {
                _context.SaldoVacaciones.Remove(saldoVacacione);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SaldoVacacioneExists(Guid id)
        {
            return _context.SaldoVacaciones.Any(e => e.Id == id);
        }
    }
}
