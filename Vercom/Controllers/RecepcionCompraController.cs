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
    public class RecepcionCompraController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public RecepcionCompraController(AppDbContext context)
        {
            _context = context;
        }

        // GET: RecepcionCompra
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.RecepcionCompras.Include(r => r.OrdenCompra);
            return View(await appDbContext.ToListAsync());
        }

        // GET: RecepcionCompra/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recepcionCompra = await _context.RecepcionCompras
                .Include(r => r.OrdenCompra)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (recepcionCompra == null)
            {
                return NotFound();
            }

            return View(recepcionCompra);
        }

        // GET: RecepcionCompra/Create
        public IActionResult Create()
        {
            ViewData["OrdenCompraId"] = new SelectList(_context.OrdenCompras, "Id", "Id");
            return View();
        }

        // POST: RecepcionCompra/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrdenCompraId,MovimientoInventarioId,CuentaPorPagarId,Fecha,NumeroInformeRecepcion,RecibidoPor")] RecepcionCompra recepcionCompra)
        {
            if (ModelState.IsValid)
            {
                recepcionCompra.Id = Guid.NewGuid();
                _context.Add(recepcionCompra);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrdenCompraId"] = new SelectList(_context.OrdenCompras, "Id", "Id", recepcionCompra.OrdenCompraId);
            return View(recepcionCompra);
        }

        // GET: RecepcionCompra/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recepcionCompra = await _context.RecepcionCompras.FindAsync(id);
            if (recepcionCompra == null)
            {
                return NotFound();
            }
            ViewData["OrdenCompraId"] = new SelectList(_context.OrdenCompras, "Id", "Id", recepcionCompra.OrdenCompraId);
            return View(recepcionCompra);
        }

        // POST: RecepcionCompra/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,OrdenCompraId,MovimientoInventarioId,CuentaPorPagarId,Fecha,NumeroInformeRecepcion,RecibidoPor")] RecepcionCompra recepcionCompra)
        {
            if (id != recepcionCompra.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recepcionCompra);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecepcionCompraExists(recepcionCompra.Id))
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
            ViewData["OrdenCompraId"] = new SelectList(_context.OrdenCompras, "Id", "Id", recepcionCompra.OrdenCompraId);
            return View(recepcionCompra);
        }

        // GET: RecepcionCompra/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recepcionCompra = await _context.RecepcionCompras
                .Include(r => r.OrdenCompra)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (recepcionCompra == null)
            {
                return NotFound();
            }

            return View(recepcionCompra);
        }

        // POST: RecepcionCompra/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var recepcionCompra = await _context.RecepcionCompras.FindAsync(id);
            if (recepcionCompra != null)
            {
                _context.RecepcionCompras.Remove(recepcionCompra);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecepcionCompraExists(Guid id)
        {
            return _context.RecepcionCompras.Any(e => e.Id == id);
        }
    }
}
