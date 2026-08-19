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
    public class CuentaPorCobrarController : Controller
    {
        private readonly AppDbContext _context;

        public CuentaPorCobrarController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CuentaPorCobrar
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.CuentaPorCobrars.Include(c => c.AsientoOrigen);
            return View(await appDbContext.ToListAsync());
        }

        // GET: CuentaPorCobrar/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentaPorCobrar = await _context.CuentaPorCobrars
                .Include(c => c.AsientoOrigen)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cuentaPorCobrar == null)
            {
                return NotFound();
            }

            return View(cuentaPorCobrar);
        }

        // GET: CuentaPorCobrar/Create
        public IActionResult Create()
        {
            ViewData["AsientoOrigenId"] = new SelectList(_context.AsientoContables, "Id", "Id");
            return View();
        }

        // POST: CuentaPorCobrar/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,ClienteId,DocumentoOrigenTipo,DocumentoOrigenId,AsientoOrigenId,FechaEmision,FechaVencimiento,MontoOriginal,SaldoPendiente,Moneda,Estado,CreadoEn")] CuentaPorCobrar cuentaPorCobrar)
        {
            if (ModelState.IsValid)
            {
                cuentaPorCobrar.Id = Guid.NewGuid();
                _context.Add(cuentaPorCobrar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AsientoOrigenId"] = new SelectList(_context.AsientoContables, "Id", "Id", cuentaPorCobrar.AsientoOrigenId);
            return View(cuentaPorCobrar);
        }

        // GET: CuentaPorCobrar/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentaPorCobrar = await _context.CuentaPorCobrars.FindAsync(id);
            if (cuentaPorCobrar == null)
            {
                return NotFound();
            }
            ViewData["AsientoOrigenId"] = new SelectList(_context.AsientoContables, "Id", "Id", cuentaPorCobrar.AsientoOrigenId);
            return View(cuentaPorCobrar);
        }

        // POST: CuentaPorCobrar/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,ClienteId,DocumentoOrigenTipo,DocumentoOrigenId,AsientoOrigenId,FechaEmision,FechaVencimiento,MontoOriginal,SaldoPendiente,Moneda,Estado,CreadoEn")] CuentaPorCobrar cuentaPorCobrar)
        {
            if (id != cuentaPorCobrar.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuentaPorCobrar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuentaPorCobrarExists(cuentaPorCobrar.Id))
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
            ViewData["AsientoOrigenId"] = new SelectList(_context.AsientoContables, "Id", "Id", cuentaPorCobrar.AsientoOrigenId);
            return View(cuentaPorCobrar);
        }

        // GET: CuentaPorCobrar/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentaPorCobrar = await _context.CuentaPorCobrars
                .Include(c => c.AsientoOrigen)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cuentaPorCobrar == null)
            {
                return NotFound();
            }

            return View(cuentaPorCobrar);
        }

        // POST: CuentaPorCobrar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var cuentaPorCobrar = await _context.CuentaPorCobrars.FindAsync(id);
            if (cuentaPorCobrar != null)
            {
                _context.CuentaPorCobrars.Remove(cuentaPorCobrar);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CuentaPorCobrarExists(Guid id)
        {
            return _context.CuentaPorCobrars.Any(e => e.Id == id);
        }
    }
}
