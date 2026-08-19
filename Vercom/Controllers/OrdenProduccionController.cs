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
    public class OrdenProduccionController : Controller
    {
        private readonly AppDbContext _context;

        public OrdenProduccionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OrdenProduccion
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.OrdenProduccions.Include(o => o.FichaCosto).Include(o => o.ListaMateriales);
            return View(await appDbContext.ToListAsync());
        }

        // GET: OrdenProduccion/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenProduccion = await _context.OrdenProduccions
                .Include(o => o.FichaCosto)
                .Include(o => o.ListaMateriales)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ordenProduccion == null)
            {
                return NotFound();
            }

            return View(ordenProduccion);
        }

        // GET: OrdenProduccion/Create
        public IActionResult Create()
        {
            ViewData["FichaCostoId"] = new SelectList(_context.FichaCostos, "Id", "Id");
            ViewData["ListaMaterialesId"] = new SelectList(_context.ListaMateriales, "Id", "Id");
            return View();
        }

        // POST: OrdenProduccion/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,NumeroOrden,ProductoTerminadoId,ListaMaterialesId,FichaCostoId,AlmacenInsumosId,AlmacenProductoId,CantidadPlanificada,CantidadProducida,FechaInicioPlan,FechaFinPlan,FechaInicioReal,FechaFinReal,Estado,CostoRealTotal,AsientoConsumoId,AsientoTerminadoId,CreadoPor,CreadoEn")] OrdenProduccion ordenProduccion)
        {
            if (ModelState.IsValid)
            {
                ordenProduccion.Id = Guid.NewGuid();
                _context.Add(ordenProduccion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FichaCostoId"] = new SelectList(_context.FichaCostos, "Id", "Id", ordenProduccion.FichaCostoId);
            ViewData["ListaMaterialesId"] = new SelectList(_context.ListaMateriales, "Id", "Id", ordenProduccion.ListaMaterialesId);
            return View(ordenProduccion);
        }

        // GET: OrdenProduccion/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenProduccion = await _context.OrdenProduccions.FindAsync(id);
            if (ordenProduccion == null)
            {
                return NotFound();
            }
            ViewData["FichaCostoId"] = new SelectList(_context.FichaCostos, "Id", "Id", ordenProduccion.FichaCostoId);
            ViewData["ListaMaterialesId"] = new SelectList(_context.ListaMateriales, "Id", "Id", ordenProduccion.ListaMaterialesId);
            return View(ordenProduccion);
        }

        // POST: OrdenProduccion/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,NumeroOrden,ProductoTerminadoId,ListaMaterialesId,FichaCostoId,AlmacenInsumosId,AlmacenProductoId,CantidadPlanificada,CantidadProducida,FechaInicioPlan,FechaFinPlan,FechaInicioReal,FechaFinReal,Estado,CostoRealTotal,AsientoConsumoId,AsientoTerminadoId,CreadoPor,CreadoEn")] OrdenProduccion ordenProduccion)
        {
            if (id != ordenProduccion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ordenProduccion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdenProduccionExists(ordenProduccion.Id))
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
            ViewData["FichaCostoId"] = new SelectList(_context.FichaCostos, "Id", "Id", ordenProduccion.FichaCostoId);
            ViewData["ListaMaterialesId"] = new SelectList(_context.ListaMateriales, "Id", "Id", ordenProduccion.ListaMaterialesId);
            return View(ordenProduccion);
        }

        // GET: OrdenProduccion/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenProduccion = await _context.OrdenProduccions
                .Include(o => o.FichaCosto)
                .Include(o => o.ListaMateriales)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ordenProduccion == null)
            {
                return NotFound();
            }

            return View(ordenProduccion);
        }

        // POST: OrdenProduccion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var ordenProduccion = await _context.OrdenProduccions.FindAsync(id);
            if (ordenProduccion != null)
            {
                _context.OrdenProduccions.Remove(ordenProduccion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdenProduccionExists(Guid id)
        {
            return _context.OrdenProduccions.Any(e => e.Id == id);
        }
    }
}
