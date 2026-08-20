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
    public class OrdenCompraDetalleController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public OrdenCompraDetalleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OrdenCompraDetalle
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.OrdenCompraDetalles.Include(o => o.OrdenCompra);
            return View(await appDbContext.ToListAsync());
        }

        // GET: OrdenCompraDetalle/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenCompraDetalle = await _context.OrdenCompraDetalles
                .Include(o => o.OrdenCompra)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ordenCompraDetalle == null)
            {
                return NotFound();
            }

            return View(ordenCompraDetalle);
        }

        // GET: OrdenCompraDetalle/Create
        public IActionResult Create()
        {
            ViewData["OrdenCompraId"] = new SelectList(_context.OrdenCompras, "Id", "Id");
            return View();
        }

        // POST: OrdenCompraDetalle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrdenCompraId,ProductoId,CantidadSolicitada,CantidadRecibida,PrecioUnitario,SubtotalLinea")] OrdenCompraDetalle ordenCompraDetalle)
        {
            if (ModelState.IsValid)
            {
                ordenCompraDetalle.Id = Guid.NewGuid();
                _context.Add(ordenCompraDetalle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrdenCompraId"] = new SelectList(_context.OrdenCompras, "Id", "Id", ordenCompraDetalle.OrdenCompraId);
            return View(ordenCompraDetalle);
        }

        // GET: OrdenCompraDetalle/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenCompraDetalle = await _context.OrdenCompraDetalles.FindAsync(id);
            if (ordenCompraDetalle == null)
            {
                return NotFound();
            }
            ViewData["OrdenCompraId"] = new SelectList(_context.OrdenCompras, "Id", "Id", ordenCompraDetalle.OrdenCompraId);
            return View(ordenCompraDetalle);
        }

        // POST: OrdenCompraDetalle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,OrdenCompraId,ProductoId,CantidadSolicitada,CantidadRecibida,PrecioUnitario,SubtotalLinea")] OrdenCompraDetalle ordenCompraDetalle)
        {
            if (id != ordenCompraDetalle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ordenCompraDetalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdenCompraDetalleExists(ordenCompraDetalle.Id))
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
            ViewData["OrdenCompraId"] = new SelectList(_context.OrdenCompras, "Id", "Id", ordenCompraDetalle.OrdenCompraId);
            return View(ordenCompraDetalle);
        }

        // GET: OrdenCompraDetalle/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenCompraDetalle = await _context.OrdenCompraDetalles
                .Include(o => o.OrdenCompra)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ordenCompraDetalle == null)
            {
                return NotFound();
            }

            return View(ordenCompraDetalle);
        }

        // POST: OrdenCompraDetalle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var ordenCompraDetalle = await _context.OrdenCompraDetalles.FindAsync(id);
            if (ordenCompraDetalle != null)
            {
                _context.OrdenCompraDetalles.Remove(ordenCompraDetalle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdenCompraDetalleExists(Guid id)
        {
            return _context.OrdenCompraDetalles.Any(e => e.Id == id);
        }
    }
}
