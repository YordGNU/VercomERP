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
    public class OrdenProduccionConsumoController : Controller
    {
        private readonly AppDbContext _context;

        public OrdenProduccionConsumoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OrdenProduccionConsumo
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.OrdenProduccionConsumos.Include(o => o.OrdenProduccion);
            return View(await appDbContext.ToListAsync());
        }

        // GET: OrdenProduccionConsumo/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenProduccionConsumo = await _context.OrdenProduccionConsumos
                .Include(o => o.OrdenProduccion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ordenProduccionConsumo == null)
            {
                return NotFound();
            }

            return View(ordenProduccionConsumo);
        }

        // GET: OrdenProduccionConsumo/Create
        public IActionResult Create()
        {
            ViewData["OrdenProduccionId"] = new SelectList(_context.OrdenProduccions, "Id", "Id");
            return View();
        }

        // POST: OrdenProduccionConsumo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrdenProduccionId,ProductoInsumoId,CantidadPlanificada,CantidadReal,CostoUnitario,MovimientoInventarioId")] OrdenProduccionConsumo ordenProduccionConsumo)
        {
            if (ModelState.IsValid)
            {
                ordenProduccionConsumo.Id = Guid.NewGuid();
                _context.Add(ordenProduccionConsumo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrdenProduccionId"] = new SelectList(_context.OrdenProduccions, "Id", "Id", ordenProduccionConsumo.OrdenProduccionId);
            return View(ordenProduccionConsumo);
        }

        // GET: OrdenProduccionConsumo/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenProduccionConsumo = await _context.OrdenProduccionConsumos.FindAsync(id);
            if (ordenProduccionConsumo == null)
            {
                return NotFound();
            }
            ViewData["OrdenProduccionId"] = new SelectList(_context.OrdenProduccions, "Id", "Id", ordenProduccionConsumo.OrdenProduccionId);
            return View(ordenProduccionConsumo);
        }

        // POST: OrdenProduccionConsumo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,OrdenProduccionId,ProductoInsumoId,CantidadPlanificada,CantidadReal,CostoUnitario,MovimientoInventarioId")] OrdenProduccionConsumo ordenProduccionConsumo)
        {
            if (id != ordenProduccionConsumo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ordenProduccionConsumo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdenProduccionConsumoExists(ordenProduccionConsumo.Id))
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
            ViewData["OrdenProduccionId"] = new SelectList(_context.OrdenProduccions, "Id", "Id", ordenProduccionConsumo.OrdenProduccionId);
            return View(ordenProduccionConsumo);
        }

        // GET: OrdenProduccionConsumo/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenProduccionConsumo = await _context.OrdenProduccionConsumos
                .Include(o => o.OrdenProduccion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ordenProduccionConsumo == null)
            {
                return NotFound();
            }

            return View(ordenProduccionConsumo);
        }

        // POST: OrdenProduccionConsumo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var ordenProduccionConsumo = await _context.OrdenProduccionConsumos.FindAsync(id);
            if (ordenProduccionConsumo != null)
            {
                _context.OrdenProduccionConsumos.Remove(ordenProduccionConsumo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdenProduccionConsumoExists(Guid id)
        {
            return _context.OrdenProduccionConsumos.Any(e => e.Id == id);
        }
    }
}
