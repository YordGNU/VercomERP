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
    public class PlanProduccionDetalleController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public PlanProduccionDetalleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PlanProduccionDetalle
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PlanProduccionDetalles.Include(p => p.Plan);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PlanProduccionDetalle/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var planProduccionDetalle = await _context.PlanProduccionDetalles
                .Include(p => p.Plan)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (planProduccionDetalle == null)
            {
                return NotFound();
            }

            return View(planProduccionDetalle);
        }

        // GET: PlanProduccionDetalle/Create
        public IActionResult Create()
        {
            ViewData["PlanId"] = new SelectList(_context.PlanProduccions, "Id", "Id");
            return View();
        }

        // POST: PlanProduccionDetalle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PlanId,ProductoId,CantidadPlanificada,CantidadEjecutada")] PlanProduccionDetalle planProduccionDetalle)
        {
            if (ModelState.IsValid)
            {
                planProduccionDetalle.Id = Guid.NewGuid();
                _context.Add(planProduccionDetalle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PlanId"] = new SelectList(_context.PlanProduccions, "Id", "Id", planProduccionDetalle.PlanId);
            return View(planProduccionDetalle);
        }

        // GET: PlanProduccionDetalle/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var planProduccionDetalle = await _context.PlanProduccionDetalles.FindAsync(id);
            if (planProduccionDetalle == null)
            {
                return NotFound();
            }
            ViewData["PlanId"] = new SelectList(_context.PlanProduccions, "Id", "Id", planProduccionDetalle.PlanId);
            return View(planProduccionDetalle);
        }

        // POST: PlanProduccionDetalle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,PlanId,ProductoId,CantidadPlanificada,CantidadEjecutada")] PlanProduccionDetalle planProduccionDetalle)
        {
            if (id != planProduccionDetalle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(planProduccionDetalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlanProduccionDetalleExists(planProduccionDetalle.Id))
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
            ViewData["PlanId"] = new SelectList(_context.PlanProduccions, "Id", "Id", planProduccionDetalle.PlanId);
            return View(planProduccionDetalle);
        }

        // GET: PlanProduccionDetalle/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var planProduccionDetalle = await _context.PlanProduccionDetalles
                .Include(p => p.Plan)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (planProduccionDetalle == null)
            {
                return NotFound();
            }

            return View(planProduccionDetalle);
        }

        // POST: PlanProduccionDetalle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var planProduccionDetalle = await _context.PlanProduccionDetalles.FindAsync(id);
            if (planProduccionDetalle != null)
            {
                _context.PlanProduccionDetalles.Remove(planProduccionDetalle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlanProduccionDetalleExists(Guid id)
        {
            return _context.PlanProduccionDetalles.Any(e => e.Id == id);
        }
    }
}
