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
    public class FacturaVentaDetalleController : Controller
    {
        private readonly AppDbContext _context;

        public FacturaVentaDetalleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: FacturaVentaDetalle
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.FacturaVentaDetalles.Include(f => f.Factura);
            return View(await appDbContext.ToListAsync());
        }

        // GET: FacturaVentaDetalle/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facturaVentaDetalle = await _context.FacturaVentaDetalles
                .Include(f => f.Factura)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (facturaVentaDetalle == null)
            {
                return NotFound();
            }

            return View(facturaVentaDetalle);
        }

        // GET: FacturaVentaDetalle/Create
        public IActionResult Create()
        {
            ViewData["FacturaId"] = new SelectList(_context.FacturaVenta, "Id", "Id");
            return View();
        }

        // POST: FacturaVentaDetalle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FacturaId,ProductoId,Cantidad,PrecioUnitario,DescuentoPorcentaje,CostoUnitarioVenta,ImpuestoPorcentaje,SubtotalLinea,MovimientoInventarioId")] FacturaVentaDetalle facturaVentaDetalle)
        {
            if (ModelState.IsValid)
            {
                facturaVentaDetalle.Id = Guid.NewGuid();
                _context.Add(facturaVentaDetalle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FacturaId"] = new SelectList(_context.FacturaVenta, "Id", "Id", facturaVentaDetalle.FacturaId);
            return View(facturaVentaDetalle);
        }

        // GET: FacturaVentaDetalle/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facturaVentaDetalle = await _context.FacturaVentaDetalles.FindAsync(id);
            if (facturaVentaDetalle == null)
            {
                return NotFound();
            }
            ViewData["FacturaId"] = new SelectList(_context.FacturaVenta, "Id", "Id", facturaVentaDetalle.FacturaId);
            return View(facturaVentaDetalle);
        }

        // POST: FacturaVentaDetalle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,FacturaId,ProductoId,Cantidad,PrecioUnitario,DescuentoPorcentaje,CostoUnitarioVenta,ImpuestoPorcentaje,SubtotalLinea,MovimientoInventarioId")] FacturaVentaDetalle facturaVentaDetalle)
        {
            if (id != facturaVentaDetalle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(facturaVentaDetalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacturaVentaDetalleExists(facturaVentaDetalle.Id))
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
            ViewData["FacturaId"] = new SelectList(_context.FacturaVenta, "Id", "Id", facturaVentaDetalle.FacturaId);
            return View(facturaVentaDetalle);
        }

        // GET: FacturaVentaDetalle/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facturaVentaDetalle = await _context.FacturaVentaDetalles
                .Include(f => f.Factura)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (facturaVentaDetalle == null)
            {
                return NotFound();
            }

            return View(facturaVentaDetalle);
        }

        // POST: FacturaVentaDetalle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var facturaVentaDetalle = await _context.FacturaVentaDetalles.FindAsync(id);
            if (facturaVentaDetalle != null)
            {
                _context.FacturaVentaDetalles.Remove(facturaVentaDetalle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FacturaVentaDetalleExists(Guid id)
        {
            return _context.FacturaVentaDetalles.Any(e => e.Id == id);
        }
    }
}
