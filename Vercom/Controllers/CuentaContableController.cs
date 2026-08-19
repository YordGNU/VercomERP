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
    public class CuentaContableController : Controller
    {
        private readonly AppDbContext _context;

        public CuentaContableController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CuentaContable
        public async Task<IActionResult> Index()
        {
            return View(await _context.CuentaContables.ToListAsync());
        }

        // GET: CuentaContable/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentaContable = await _context.CuentaContables
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cuentaContable == null)
            {
                return NotFound();
            }

            return View(cuentaContable);
        }

        // GET: CuentaContable/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CuentaContable/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,Codigo,Nombre,CuentaPadreId,Nivel,Clase,Naturaleza,AceptaMovimiento,RequiereCentroCosto,RequiereTercero,Moneda,Activo,CreadoEn")] CuentaContable cuentaContable)
        {
            if (ModelState.IsValid)
            {
                cuentaContable.Id = Guid.NewGuid();
                _context.Add(cuentaContable);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cuentaContable);
        }

        // GET: CuentaContable/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentaContable = await _context.CuentaContables.FindAsync(id);
            if (cuentaContable == null)
            {
                return NotFound();
            }
            return View(cuentaContable);
        }

        // POST: CuentaContable/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,Codigo,Nombre,CuentaPadreId,Nivel,Clase,Naturaleza,AceptaMovimiento,RequiereCentroCosto,RequiereTercero,Moneda,Activo,CreadoEn")] CuentaContable cuentaContable)
        {
            if (id != cuentaContable.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuentaContable);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuentaContableExists(cuentaContable.Id))
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
            return View(cuentaContable);
        }

        // GET: CuentaContable/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentaContable = await _context.CuentaContables
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cuentaContable == null)
            {
                return NotFound();
            }

            return View(cuentaContable);
        }

        // POST: CuentaContable/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var cuentaContable = await _context.CuentaContables.FindAsync(id);
            if (cuentaContable != null)
            {
                _context.CuentaContables.Remove(cuentaContable);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CuentaContableExists(Guid id)
        {
            return _context.CuentaContables.Any(e => e.Id == id);
        }
    }
}
