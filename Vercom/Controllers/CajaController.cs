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
    public class CajaController : Controller
    {
        private readonly AppDbContext _context;

        public CajaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Caja
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Cajas.Include(c => c.CuentaContable);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Caja/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caja = await _context.Cajas
                .Include(c => c.CuentaContable)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (caja == null)
            {
                return NotFound();
            }

            return View(caja);
        }

        // GET: Caja/Create
        public IActionResult Create()
        {
            ViewData["CuentaContableId"] = new SelectList(_context.CuentaContables, "Id", "Id");
            return View();
        }

        // POST: Caja/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,SucursalId,Nombre,CuentaContableId,LimiteEfectivo,SaldoActual,Activa")] Caja caja)
        {
            if (ModelState.IsValid)
            {
                caja.Id = Guid.NewGuid();
                _context.Add(caja);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CuentaContableId"] = new SelectList(_context.CuentaContables, "Id", "Id", caja.CuentaContableId);
            return View(caja);
        }

        // GET: Caja/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caja = await _context.Cajas.FindAsync(id);
            if (caja == null)
            {
                return NotFound();
            }
            ViewData["CuentaContableId"] = new SelectList(_context.CuentaContables, "Id", "Id", caja.CuentaContableId);
            return View(caja);
        }

        // POST: Caja/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,SucursalId,Nombre,CuentaContableId,LimiteEfectivo,SaldoActual,Activa")] Caja caja)
        {
            if (id != caja.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(caja);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CajaExists(caja.Id))
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
            ViewData["CuentaContableId"] = new SelectList(_context.CuentaContables, "Id", "Id", caja.CuentaContableId);
            return View(caja);
        }

        // GET: Caja/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caja = await _context.Cajas
                .Include(c => c.CuentaContable)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (caja == null)
            {
                return NotFound();
            }

            return View(caja);
        }

        // POST: Caja/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var caja = await _context.Cajas.FindAsync(id);
            if (caja != null)
            {
                _context.Cajas.Remove(caja);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CajaExists(Guid id)
        {
            return _context.Cajas.Any(e => e.Id == id);
        }
    }
}
