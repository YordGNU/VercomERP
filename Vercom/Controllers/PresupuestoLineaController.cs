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
    public class PresupuestoLineaController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public PresupuestoLineaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PresupuestoLinea
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PresupuestoLineas.Include(p => p.CentroCosto).Include(p => p.Cuenta).Include(p => p.Presupuesto);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PresupuestoLinea/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var presupuestoLinea = await _context.PresupuestoLineas
                .Include(p => p.CentroCosto)
                .Include(p => p.Cuenta)
                .Include(p => p.Presupuesto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (presupuestoLinea == null)
            {
                return NotFound();
            }

            return View(presupuestoLinea);
        }

        // GET: PresupuestoLinea/Create
        public IActionResult Create()
        {
            ViewData["CentroCostoId"] = new SelectList(_context.CentroCostos, "Id", "Id");
            ViewData["CuentaId"] = new SelectList(_context.CuentaContables, "Id", "Id");
            ViewData["PresupuestoId"] = new SelectList(_context.Presupuestos, "Id", "Id");
            return View();
        }

        // POST: PresupuestoLinea/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PresupuestoId,CuentaId,CentroCostoId,Mes,MontoPlanificado")] PresupuestoLinea presupuestoLinea)
        {
            if (ModelState.IsValid)
            {
                presupuestoLinea.Id = Guid.NewGuid();
                _context.Add(presupuestoLinea);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CentroCostoId"] = new SelectList(_context.CentroCostos, "Id", "Id", presupuestoLinea.CentroCostoId);
            ViewData["CuentaId"] = new SelectList(_context.CuentaContables, "Id", "Id", presupuestoLinea.CuentaId);
            ViewData["PresupuestoId"] = new SelectList(_context.Presupuestos, "Id", "Id", presupuestoLinea.PresupuestoId);
            return View(presupuestoLinea);
        }

        // GET: PresupuestoLinea/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var presupuestoLinea = await _context.PresupuestoLineas.FindAsync(id);
            if (presupuestoLinea == null)
            {
                return NotFound();
            }
            ViewData["CentroCostoId"] = new SelectList(_context.CentroCostos, "Id", "Id", presupuestoLinea.CentroCostoId);
            ViewData["CuentaId"] = new SelectList(_context.CuentaContables, "Id", "Id", presupuestoLinea.CuentaId);
            ViewData["PresupuestoId"] = new SelectList(_context.Presupuestos, "Id", "Id", presupuestoLinea.PresupuestoId);
            return View(presupuestoLinea);
        }

        // POST: PresupuestoLinea/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,PresupuestoId,CuentaId,CentroCostoId,Mes,MontoPlanificado")] PresupuestoLinea presupuestoLinea)
        {
            if (id != presupuestoLinea.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(presupuestoLinea);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PresupuestoLineaExists(presupuestoLinea.Id))
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
            ViewData["CentroCostoId"] = new SelectList(_context.CentroCostos, "Id", "Id", presupuestoLinea.CentroCostoId);
            ViewData["CuentaId"] = new SelectList(_context.CuentaContables, "Id", "Id", presupuestoLinea.CuentaId);
            ViewData["PresupuestoId"] = new SelectList(_context.Presupuestos, "Id", "Id", presupuestoLinea.PresupuestoId);
            return View(presupuestoLinea);
        }

        // GET: PresupuestoLinea/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var presupuestoLinea = await _context.PresupuestoLineas
                .Include(p => p.CentroCosto)
                .Include(p => p.Cuenta)
                .Include(p => p.Presupuesto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (presupuestoLinea == null)
            {
                return NotFound();
            }

            return View(presupuestoLinea);
        }

        // POST: PresupuestoLinea/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var presupuestoLinea = await _context.PresupuestoLineas.FindAsync(id);
            if (presupuestoLinea != null)
            {
                _context.PresupuestoLineas.Remove(presupuestoLinea);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PresupuestoLineaExists(Guid id)
        {
            return _context.PresupuestoLineas.Any(e => e.Id == id);
        }
    }
}
