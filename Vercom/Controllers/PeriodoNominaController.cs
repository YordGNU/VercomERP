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
    public class PeriodoNominaController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public PeriodoNominaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PeriodoNomina
        public async Task<IActionResult> Index()
        {
            return View(await _context.PeriodoNominas.ToListAsync());
        }

        // GET: PeriodoNomina/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var periodoNomina = await _context.PeriodoNominas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (periodoNomina == null)
            {
                return NotFound();
            }

            return View(periodoNomina);
        }

        // GET: PeriodoNomina/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PeriodoNomina/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,Anio,Mes,Tipo,Estado,AsientoId,CalculadoEn,AprobadoPor")] PeriodoNomina periodoNomina)
        {
            if (ModelState.IsValid)
            {
                periodoNomina.Id = Guid.NewGuid();
                _context.Add(periodoNomina);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(periodoNomina);
        }

        // GET: PeriodoNomina/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var periodoNomina = await _context.PeriodoNominas.FindAsync(id);
            if (periodoNomina == null)
            {
                return NotFound();
            }
            return View(periodoNomina);
        }

        // POST: PeriodoNomina/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,Anio,Mes,Tipo,Estado,AsientoId,CalculadoEn,AprobadoPor")] PeriodoNomina periodoNomina)
        {
            if (id != periodoNomina.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(periodoNomina);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PeriodoNominaExists(periodoNomina.Id))
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
            return View(periodoNomina);
        }

        // GET: PeriodoNomina/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var periodoNomina = await _context.PeriodoNominas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (periodoNomina == null)
            {
                return NotFound();
            }

            return View(periodoNomina);
        }

        // POST: PeriodoNomina/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var periodoNomina = await _context.PeriodoNominas.FindAsync(id);
            if (periodoNomina != null)
            {
                _context.PeriodoNominas.Remove(periodoNomina);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PeriodoNominaExists(Guid id)
        {
            return _context.PeriodoNominas.Any(e => e.Id == id);
        }
    }
}
