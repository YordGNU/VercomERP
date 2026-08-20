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
    public class AsientoDetalleController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public AsientoDetalleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AsientoDetalle
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.AsientoDetalles.Include(a => a.Asiento).Include(a => a.CentroCosto).Include(a => a.Cuenta);
            return View(await appDbContext.ToListAsync());
        }

        // GET: AsientoDetalle/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asientoDetalle = await _context.AsientoDetalles
                .Include(a => a.Asiento)
                .Include(a => a.CentroCosto)
                .Include(a => a.Cuenta)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asientoDetalle == null)
            {
                return NotFound();
            }

            return View(asientoDetalle);
        }

        // GET: AsientoDetalle/Create
        public IActionResult Create()
        {
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id");
            ViewData["CentroCostoId"] = new SelectList(_context.CentroCostos, "Id", "Id");
            ViewData["CuentaId"] = new SelectList(_context.CuentaContables, "Id", "Id");
            return View();
        }

        // POST: AsientoDetalle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AsientoId,Linea,CuentaId,CentroCostoId,TerceroTipo,TerceroId,Debe,Haber,Glosa")] AsientoDetalle asientoDetalle)
        {
            if (ModelState.IsValid)
            {
                asientoDetalle.Id = Guid.NewGuid();
                _context.Add(asientoDetalle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id", asientoDetalle.AsientoId);
            ViewData["CentroCostoId"] = new SelectList(_context.CentroCostos, "Id", "Id", asientoDetalle.CentroCostoId);
            ViewData["CuentaId"] = new SelectList(_context.CuentaContables, "Id", "Id", asientoDetalle.CuentaId);
            return View(asientoDetalle);
        }

        // GET: AsientoDetalle/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asientoDetalle = await _context.AsientoDetalles.FindAsync(id);
            if (asientoDetalle == null)
            {
                return NotFound();
            }
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id", asientoDetalle.AsientoId);
            ViewData["CentroCostoId"] = new SelectList(_context.CentroCostos, "Id", "Id", asientoDetalle.CentroCostoId);
            ViewData["CuentaId"] = new SelectList(_context.CuentaContables, "Id", "Id", asientoDetalle.CuentaId);
            return View(asientoDetalle);
        }

        // POST: AsientoDetalle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,AsientoId,Linea,CuentaId,CentroCostoId,TerceroTipo,TerceroId,Debe,Haber,Glosa")] AsientoDetalle asientoDetalle)
        {
            if (id != asientoDetalle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(asientoDetalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AsientoDetalleExists(asientoDetalle.Id))
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
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id", asientoDetalle.AsientoId);
            ViewData["CentroCostoId"] = new SelectList(_context.CentroCostos, "Id", "Id", asientoDetalle.CentroCostoId);
            ViewData["CuentaId"] = new SelectList(_context.CuentaContables, "Id", "Id", asientoDetalle.CuentaId);
            return View(asientoDetalle);
        }

        // GET: AsientoDetalle/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asientoDetalle = await _context.AsientoDetalles
                .Include(a => a.Asiento)
                .Include(a => a.CentroCosto)
                .Include(a => a.Cuenta)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asientoDetalle == null)
            {
                return NotFound();
            }

            return View(asientoDetalle);
        }

        // POST: AsientoDetalle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var asientoDetalle = await _context.AsientoDetalles.FindAsync(id);
            if (asientoDetalle != null)
            {
                _context.AsientoDetalles.Remove(asientoDetalle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AsientoDetalleExists(Guid id)
        {
            return _context.AsientoDetalles.Any(e => e.Id == id);
        }
    }
}
