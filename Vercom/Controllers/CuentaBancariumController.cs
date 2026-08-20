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
    public class CuentaBancariumController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public CuentaBancariumController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CuentaBancarium
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.CuentaBancaria.Include(c => c.CuentaContable);
            return View(await appDbContext.ToListAsync());
        }

        // GET: CuentaBancarium/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentaBancarium = await _context.CuentaBancaria
                .Include(c => c.CuentaContable)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cuentaBancarium == null)
            {
                return NotFound();
            }

            return View(cuentaBancarium);
        }

        // GET: CuentaBancarium/Create
        public IActionResult Create()
        {
            ViewData["CuentaContableId"] = new SelectList(_context.CuentaContables, "Id", "Id");
            return View();
        }

        // POST: CuentaBancarium/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,Banco,NumeroCuenta,TipoCuenta,CuentaContableId,SaldoActual,Activa")] CuentaBancarium cuentaBancarium)
        {
            if (ModelState.IsValid)
            {
                cuentaBancarium.Id = Guid.NewGuid();
                _context.Add(cuentaBancarium);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CuentaContableId"] = new SelectList(_context.CuentaContables, "Id", "Id", cuentaBancarium.CuentaContableId);
            return View(cuentaBancarium);
        }

        // GET: CuentaBancarium/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentaBancarium = await _context.CuentaBancaria.FindAsync(id);
            if (cuentaBancarium == null)
            {
                return NotFound();
            }
            ViewData["CuentaContableId"] = new SelectList(_context.CuentaContables, "Id", "Id", cuentaBancarium.CuentaContableId);
            return View(cuentaBancarium);
        }

        // POST: CuentaBancarium/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,Banco,NumeroCuenta,TipoCuenta,CuentaContableId,SaldoActual,Activa")] CuentaBancarium cuentaBancarium)
        {
            if (id != cuentaBancarium.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuentaBancarium);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuentaBancariumExists(cuentaBancarium.Id))
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
            ViewData["CuentaContableId"] = new SelectList(_context.CuentaContables, "Id", "Id", cuentaBancarium.CuentaContableId);
            return View(cuentaBancarium);
        }

        // GET: CuentaBancarium/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentaBancarium = await _context.CuentaBancaria
                .Include(c => c.CuentaContable)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cuentaBancarium == null)
            {
                return NotFound();
            }

            return View(cuentaBancarium);
        }

        // POST: CuentaBancarium/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var cuentaBancarium = await _context.CuentaBancaria.FindAsync(id);
            if (cuentaBancarium != null)
            {
                _context.CuentaBancaria.Remove(cuentaBancarium);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CuentaBancariumExists(Guid id)
        {
            return _context.CuentaBancaria.Any(e => e.Id == id);
        }
    }
}
