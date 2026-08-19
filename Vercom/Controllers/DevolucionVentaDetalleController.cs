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
    public class DevolucionVentaDetalleController : Controller
    {
        private readonly AppDbContext _context;

        public DevolucionVentaDetalleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DevolucionVentaDetalle
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.DevolucionVentaDetalles.Include(d => d.Devolucion).Include(d => d.FacturaDetalle);
            return View(await appDbContext.ToListAsync());
        }

        // GET: DevolucionVentaDetalle/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var devolucionVentaDetalle = await _context.DevolucionVentaDetalles
                .Include(d => d.Devolucion)
                .Include(d => d.FacturaDetalle)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (devolucionVentaDetalle == null)
            {
                return NotFound();
            }

            return View(devolucionVentaDetalle);
        }

        // GET: DevolucionVentaDetalle/Create
        public IActionResult Create()
        {
            ViewData["DevolucionId"] = new SelectList(_context.DevolucionVenta, "Id", "Id");
            ViewData["FacturaDetalleId"] = new SelectList(_context.FacturaVentaDetalles, "Id", "Id");
            return View();
        }

        // POST: DevolucionVentaDetalle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DevolucionId,FacturaDetalleId,CantidadDevuelta")] DevolucionVentaDetalle devolucionVentaDetalle)
        {
            if (ModelState.IsValid)
            {
                devolucionVentaDetalle.Id = Guid.NewGuid();
                _context.Add(devolucionVentaDetalle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DevolucionId"] = new SelectList(_context.DevolucionVenta, "Id", "Id", devolucionVentaDetalle.DevolucionId);
            ViewData["FacturaDetalleId"] = new SelectList(_context.FacturaVentaDetalles, "Id", "Id", devolucionVentaDetalle.FacturaDetalleId);
            return View(devolucionVentaDetalle);
        }

        // GET: DevolucionVentaDetalle/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var devolucionVentaDetalle = await _context.DevolucionVentaDetalles.FindAsync(id);
            if (devolucionVentaDetalle == null)
            {
                return NotFound();
            }
            ViewData["DevolucionId"] = new SelectList(_context.DevolucionVenta, "Id", "Id", devolucionVentaDetalle.DevolucionId);
            ViewData["FacturaDetalleId"] = new SelectList(_context.FacturaVentaDetalles, "Id", "Id", devolucionVentaDetalle.FacturaDetalleId);
            return View(devolucionVentaDetalle);
        }

        // POST: DevolucionVentaDetalle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,DevolucionId,FacturaDetalleId,CantidadDevuelta")] DevolucionVentaDetalle devolucionVentaDetalle)
        {
            if (id != devolucionVentaDetalle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(devolucionVentaDetalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DevolucionVentaDetalleExists(devolucionVentaDetalle.Id))
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
            ViewData["DevolucionId"] = new SelectList(_context.DevolucionVenta, "Id", "Id", devolucionVentaDetalle.DevolucionId);
            ViewData["FacturaDetalleId"] = new SelectList(_context.FacturaVentaDetalles, "Id", "Id", devolucionVentaDetalle.FacturaDetalleId);
            return View(devolucionVentaDetalle);
        }

        // GET: DevolucionVentaDetalle/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var devolucionVentaDetalle = await _context.DevolucionVentaDetalles
                .Include(d => d.Devolucion)
                .Include(d => d.FacturaDetalle)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (devolucionVentaDetalle == null)
            {
                return NotFound();
            }

            return View(devolucionVentaDetalle);
        }

        // POST: DevolucionVentaDetalle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var devolucionVentaDetalle = await _context.DevolucionVentaDetalles.FindAsync(id);
            if (devolucionVentaDetalle != null)
            {
                _context.DevolucionVentaDetalles.Remove(devolucionVentaDetalle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DevolucionVentaDetalleExists(Guid id)
        {
            return _context.DevolucionVentaDetalles.Any(e => e.Id == id);
        }
    }
}
