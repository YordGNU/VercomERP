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
    public class OrdenCompraController : Controller
    {
        private readonly AppDbContext _context;

        public OrdenCompraController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OrdenCompra
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.OrdenCompras.Include(o => o.Contrato).Include(o => o.Proveedor);
            return View(await appDbContext.ToListAsync());
        }

        // GET: OrdenCompra/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenCompra = await _context.OrdenCompras
                .Include(o => o.Contrato)
                .Include(o => o.Proveedor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ordenCompra == null)
            {
                return NotFound();
            }

            return View(ordenCompra);
        }

        // GET: OrdenCompra/Create
        public IActionResult Create()
        {
            ViewData["ContratoId"] = new SelectList(_context.ContratoEconomicos, "Id", "Id");
            ViewData["ProveedorId"] = new SelectList(_context.Proveedors, "Id", "Id");
            return View();
        }

        // POST: OrdenCompra/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,NumeroOrden,ProveedorId,ContratoId,AlmacenDestinoId,Fecha,FechaEntregaEsperada,Moneda,Subtotal,Total,Estado,AprobadoPor,CreadoPor,CreadoEn")] OrdenCompra ordenCompra)
        {
            if (ModelState.IsValid)
            {
                ordenCompra.Id = Guid.NewGuid();
                _context.Add(ordenCompra);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ContratoId"] = new SelectList(_context.ContratoEconomicos, "Id", "Id", ordenCompra.ContratoId);
            ViewData["ProveedorId"] = new SelectList(_context.Proveedors, "Id", "Id", ordenCompra.ProveedorId);
            return View(ordenCompra);
        }

        // GET: OrdenCompra/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenCompra = await _context.OrdenCompras.FindAsync(id);
            if (ordenCompra == null)
            {
                return NotFound();
            }
            ViewData["ContratoId"] = new SelectList(_context.ContratoEconomicos, "Id", "Id", ordenCompra.ContratoId);
            ViewData["ProveedorId"] = new SelectList(_context.Proveedors, "Id", "Id", ordenCompra.ProveedorId);
            return View(ordenCompra);
        }

        // POST: OrdenCompra/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,NumeroOrden,ProveedorId,ContratoId,AlmacenDestinoId,Fecha,FechaEntregaEsperada,Moneda,Subtotal,Total,Estado,AprobadoPor,CreadoPor,CreadoEn")] OrdenCompra ordenCompra)
        {
            if (id != ordenCompra.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ordenCompra);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdenCompraExists(ordenCompra.Id))
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
            ViewData["ContratoId"] = new SelectList(_context.ContratoEconomicos, "Id", "Id", ordenCompra.ContratoId);
            ViewData["ProveedorId"] = new SelectList(_context.Proveedors, "Id", "Id", ordenCompra.ProveedorId);
            return View(ordenCompra);
        }

        // GET: OrdenCompra/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenCompra = await _context.OrdenCompras
                .Include(o => o.Contrato)
                .Include(o => o.Proveedor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ordenCompra == null)
            {
                return NotFound();
            }

            return View(ordenCompra);
        }

        // POST: OrdenCompra/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var ordenCompra = await _context.OrdenCompras.FindAsync(id);
            if (ordenCompra != null)
            {
                _context.OrdenCompras.Remove(ordenCompra);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdenCompraExists(Guid id)
        {
            return _context.OrdenCompras.Any(e => e.Id == id);
        }
    }
}
