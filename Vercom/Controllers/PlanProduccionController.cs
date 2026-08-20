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
    public class PlanProduccionController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public PlanProduccionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PlanProduccion
        public async Task<IActionResult> Index()
        {
            return View(await _context.PlanProduccions.ToListAsync());
        }

        // GET: PlanProduccion/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var planProduccion = await _context.PlanProduccions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (planProduccion == null)
            {
                return NotFound();
            }

            return View(planProduccion);
        }

        // GET: PlanProduccion/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PlanProduccion/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,Anio,Mes,PresupuestoId,Estado,CreadoEn")] PlanProduccion planProduccion)
        {
            if (ModelState.IsValid)
            {
                planProduccion.Id = Guid.NewGuid();
                _context.Add(planProduccion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(planProduccion);
        }

        // GET: PlanProduccion/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var planProduccion = await _context.PlanProduccions.FindAsync(id);
            if (planProduccion == null)
            {
                return NotFound();
            }
            return View(planProduccion);
        }

        // POST: PlanProduccion/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,Anio,Mes,PresupuestoId,Estado,CreadoEn")] PlanProduccion planProduccion)
        {
            if (id != planProduccion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(planProduccion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlanProduccionExists(planProduccion.Id))
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
            return View(planProduccion);
        }

        // GET: PlanProduccion/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var planProduccion = await _context.PlanProduccions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (planProduccion == null)
            {
                return NotFound();
            }

            return View(planProduccion);
        }

        // POST: PlanProduccion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var planProduccion = await _context.PlanProduccions.FindAsync(id);
            if (planProduccion != null)
            {
                _context.PlanProduccions.Remove(planProduccion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlanProduccionExists(Guid id)
        {
            return _context.PlanProduccions.Any(e => e.Id == id);
        }
    }
}
