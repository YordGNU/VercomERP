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
    public class CuentaPorPagarController : Controller
    {
        private readonly AppDbContext _context;

        public CuentaPorPagarController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CuentaPorPagar
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.CuentaPorPagars.Include(c => c.AsientoOrigen);
            return View(await appDbContext.ToListAsync());
        }

        // GET: CuentaPorPagar/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentaPorPagar = await _context.CuentaPorPagars
                .Include(c => c.AsientoOrigen)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cuentaPorPagar == null)
            {
                return NotFound();
            }

            return View(cuentaPorPagar);
        }

        // GET: CuentaPorPagar/Create
        public IActionResult Create()
        {
            ViewData["AsientoOrigenId"] = new SelectList(_context.AsientoContables, "Id", "Id");
            return View();
        }

        // POST: CuentaPorPagar/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,ProveedorId,DocumentoOrigenTipo,DocumentoOrigenId,AsientoOrigenId,FechaEmision,FechaVencimiento,MontoOriginal,SaldoPendiente,Moneda,Estado,CreadoEn")] CuentaPorPagar cuentaPorPagar)
        {
            if (ModelState.IsValid)
            {
                cuentaPorPagar.Id = Guid.NewGuid();
                _context.Add(cuentaPorPagar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AsientoOrigenId"] = new SelectList(_context.AsientoContables, "Id", "Id", cuentaPorPagar.AsientoOrigenId);
            return View(cuentaPorPagar);
        }

        // GET: CuentaPorPagar/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentaPorPagar = await _context.CuentaPorPagars.FindAsync(id);
            if (cuentaPorPagar == null)
            {
                return NotFound();
            }
            ViewData["AsientoOrigenId"] = new SelectList(_context.AsientoContables, "Id", "Id", cuentaPorPagar.AsientoOrigenId);
            return View(cuentaPorPagar);
        }

        // POST: CuentaPorPagar/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,ProveedorId,DocumentoOrigenTipo,DocumentoOrigenId,AsientoOrigenId,FechaEmision,FechaVencimiento,MontoOriginal,SaldoPendiente,Moneda,Estado,CreadoEn")] CuentaPorPagar cuentaPorPagar)
        {
            if (id != cuentaPorPagar.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuentaPorPagar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuentaPorPagarExists(cuentaPorPagar.Id))
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
            ViewData["AsientoOrigenId"] = new SelectList(_context.AsientoContables, "Id", "Id", cuentaPorPagar.AsientoOrigenId);
            return View(cuentaPorPagar);
        }

        // GET: CuentaPorPagar/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentaPorPagar = await _context.CuentaPorPagars
                .Include(c => c.AsientoOrigen)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cuentaPorPagar == null)
            {
                return NotFound();
            }

            return View(cuentaPorPagar);
        }

        // POST: CuentaPorPagar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var cuentaPorPagar = await _context.CuentaPorPagars.FindAsync(id);
            if (cuentaPorPagar != null)
            {
                _context.CuentaPorPagars.Remove(cuentaPorPagar);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CuentaPorPagarExists(Guid id)
        {
            return _context.CuentaPorPagars.Any(e => e.Id == id);
        }
    }
}
