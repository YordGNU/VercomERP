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
    public class PagoAplicadoController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public PagoAplicadoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PagoAplicado
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PagoAplicados.Include(p => p.Asiento).Include(p => p.CuentaPorCobrar).Include(p => p.CuentaPorPagar);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PagoAplicado/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagoAplicado = await _context.PagoAplicados
                .Include(p => p.Asiento)
                .Include(p => p.CuentaPorCobrar)
                .Include(p => p.CuentaPorPagar)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pagoAplicado == null)
            {
                return NotFound();
            }

            return View(pagoAplicado);
        }

        // GET: PagoAplicado/Create
        public IActionResult Create()
        {
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id");
            ViewData["CuentaPorCobrarId"] = new SelectList(_context.CuentaPorCobrars, "Id", "Id");
            ViewData["CuentaPorPagarId"] = new SelectList(_context.CuentaPorPagars, "Id", "Id");
            return View();
        }

        // POST: PagoAplicado/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Tipo,CuentaPorCobrarId,CuentaPorPagarId,Fecha,Monto,FormaPago,AsientoId,ReferenciaExterna")] PagoAplicado pagoAplicado)
        {
            if (ModelState.IsValid)
            {
                pagoAplicado.Id = Guid.NewGuid();
                _context.Add(pagoAplicado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id", pagoAplicado.AsientoId);
            ViewData["CuentaPorCobrarId"] = new SelectList(_context.CuentaPorCobrars, "Id", "Id", pagoAplicado.CuentaPorCobrarId);
            ViewData["CuentaPorPagarId"] = new SelectList(_context.CuentaPorPagars, "Id", "Id", pagoAplicado.CuentaPorPagarId);
            return View(pagoAplicado);
        }

        // GET: PagoAplicado/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagoAplicado = await _context.PagoAplicados.FindAsync(id);
            if (pagoAplicado == null)
            {
                return NotFound();
            }
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id", pagoAplicado.AsientoId);
            ViewData["CuentaPorCobrarId"] = new SelectList(_context.CuentaPorCobrars, "Id", "Id", pagoAplicado.CuentaPorCobrarId);
            ViewData["CuentaPorPagarId"] = new SelectList(_context.CuentaPorPagars, "Id", "Id", pagoAplicado.CuentaPorPagarId);
            return View(pagoAplicado);
        }

        // POST: PagoAplicado/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Tipo,CuentaPorCobrarId,CuentaPorPagarId,Fecha,Monto,FormaPago,AsientoId,ReferenciaExterna")] PagoAplicado pagoAplicado)
        {
            if (id != pagoAplicado.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pagoAplicado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PagoAplicadoExists(pagoAplicado.Id))
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
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id", pagoAplicado.AsientoId);
            ViewData["CuentaPorCobrarId"] = new SelectList(_context.CuentaPorCobrars, "Id", "Id", pagoAplicado.CuentaPorCobrarId);
            ViewData["CuentaPorPagarId"] = new SelectList(_context.CuentaPorPagars, "Id", "Id", pagoAplicado.CuentaPorPagarId);
            return View(pagoAplicado);
        }

        // GET: PagoAplicado/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagoAplicado = await _context.PagoAplicados
                .Include(p => p.Asiento)
                .Include(p => p.CuentaPorCobrar)
                .Include(p => p.CuentaPorPagar)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pagoAplicado == null)
            {
                return NotFound();
            }

            return View(pagoAplicado);
        }

        // POST: PagoAplicado/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var pagoAplicado = await _context.PagoAplicados.FindAsync(id);
            if (pagoAplicado != null)
            {
                _context.PagoAplicados.Remove(pagoAplicado);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PagoAplicadoExists(Guid id)
        {
            return _context.PagoAplicados.Any(e => e.Id == id);
        }
    }
}
