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
    public class MovimientoInventarioDetalleController : Controller
    {
        private readonly AppDbContext _context;

        public MovimientoInventarioDetalleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: MovimientoInventarioDetalle
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.MovimientoInventarioDetalles.Include(m => m.Movimiento).Include(m => m.Producto);
            return View(await appDbContext.ToListAsync());
        }

        // GET: MovimientoInventarioDetalle/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimientoInventarioDetalle = await _context.MovimientoInventarioDetalles
                .Include(m => m.Movimiento)
                .Include(m => m.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movimientoInventarioDetalle == null)
            {
                return NotFound();
            }

            return View(movimientoInventarioDetalle);
        }

        // GET: MovimientoInventarioDetalle/Create
        public IActionResult Create()
        {
            ViewData["MovimientoId"] = new SelectList(_context.MovimientoInventarios, "Id", "Id");
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id");
            return View();
        }

        // POST: MovimientoInventarioDetalle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MovimientoId,ProductoId,Cantidad,CostoUnitario,Lote,FechaVencimiento,Observaciones")] MovimientoInventarioDetalle movimientoInventarioDetalle)
        {
            if (ModelState.IsValid)
            {
                movimientoInventarioDetalle.Id = Guid.NewGuid();
                _context.Add(movimientoInventarioDetalle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MovimientoId"] = new SelectList(_context.MovimientoInventarios, "Id", "Id", movimientoInventarioDetalle.MovimientoId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id", movimientoInventarioDetalle.ProductoId);
            return View(movimientoInventarioDetalle);
        }

        // GET: MovimientoInventarioDetalle/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimientoInventarioDetalle = await _context.MovimientoInventarioDetalles.FindAsync(id);
            if (movimientoInventarioDetalle == null)
            {
                return NotFound();
            }
            ViewData["MovimientoId"] = new SelectList(_context.MovimientoInventarios, "Id", "Id", movimientoInventarioDetalle.MovimientoId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id", movimientoInventarioDetalle.ProductoId);
            return View(movimientoInventarioDetalle);
        }

        // POST: MovimientoInventarioDetalle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,MovimientoId,ProductoId,Cantidad,CostoUnitario,Lote,FechaVencimiento,Observaciones")] MovimientoInventarioDetalle movimientoInventarioDetalle)
        {
            if (id != movimientoInventarioDetalle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movimientoInventarioDetalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovimientoInventarioDetalleExists(movimientoInventarioDetalle.Id))
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
            ViewData["MovimientoId"] = new SelectList(_context.MovimientoInventarios, "Id", "Id", movimientoInventarioDetalle.MovimientoId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id", movimientoInventarioDetalle.ProductoId);
            return View(movimientoInventarioDetalle);
        }

        // GET: MovimientoInventarioDetalle/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimientoInventarioDetalle = await _context.MovimientoInventarioDetalles
                .Include(m => m.Movimiento)
                .Include(m => m.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movimientoInventarioDetalle == null)
            {
                return NotFound();
            }

            return View(movimientoInventarioDetalle);
        }

        // POST: MovimientoInventarioDetalle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var movimientoInventarioDetalle = await _context.MovimientoInventarioDetalles.FindAsync(id);
            if (movimientoInventarioDetalle != null)
            {
                _context.MovimientoInventarioDetalles.Remove(movimientoInventarioDetalle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovimientoInventarioDetalleExists(Guid id)
        {
            return _context.MovimientoInventarioDetalles.Any(e => e.Id == id);
        }
    }
}
