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
    public class MovimientoBancarioController : Controller
    {
        private readonly AppDbContext _context;

        public MovimientoBancarioController(AppDbContext context)
        {
            _context = context;
        }

        // GET: MovimientoBancario
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.MovimientoBancarios.Include(m => m.Asiento).Include(m => m.CuentaBancaria);
            return View(await appDbContext.ToListAsync());
        }

        // GET: MovimientoBancario/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimientoBancario = await _context.MovimientoBancarios
                .Include(m => m.Asiento)
                .Include(m => m.CuentaBancaria)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movimientoBancario == null)
            {
                return NotFound();
            }

            return View(movimientoBancario);
        }

        // GET: MovimientoBancario/Create
        public IActionResult Create()
        {
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id");
            ViewData["CuentaBancariaId"] = new SelectList(_context.CuentaBancaria, "Id", "Id");
            return View();
        }

        // POST: MovimientoBancario/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CuentaBancariaId,Fecha,Tipo,Monto,Descripcion,Referencia,Conciliado,FechaConciliacion,AsientoId")] MovimientoBancario movimientoBancario)
        {
            if (ModelState.IsValid)
            {
                movimientoBancario.Id = Guid.NewGuid();
                _context.Add(movimientoBancario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id", movimientoBancario.AsientoId);
            ViewData["CuentaBancariaId"] = new SelectList(_context.CuentaBancaria, "Id", "Id", movimientoBancario.CuentaBancariaId);
            return View(movimientoBancario);
        }

        // GET: MovimientoBancario/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimientoBancario = await _context.MovimientoBancarios.FindAsync(id);
            if (movimientoBancario == null)
            {
                return NotFound();
            }
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id", movimientoBancario.AsientoId);
            ViewData["CuentaBancariaId"] = new SelectList(_context.CuentaBancaria, "Id", "Id", movimientoBancario.CuentaBancariaId);
            return View(movimientoBancario);
        }

        // POST: MovimientoBancario/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,CuentaBancariaId,Fecha,Tipo,Monto,Descripcion,Referencia,Conciliado,FechaConciliacion,AsientoId")] MovimientoBancario movimientoBancario)
        {
            if (id != movimientoBancario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movimientoBancario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovimientoBancarioExists(movimientoBancario.Id))
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
            ViewData["AsientoId"] = new SelectList(_context.AsientoContables, "Id", "Id", movimientoBancario.AsientoId);
            ViewData["CuentaBancariaId"] = new SelectList(_context.CuentaBancaria, "Id", "Id", movimientoBancario.CuentaBancariaId);
            return View(movimientoBancario);
        }

        // GET: MovimientoBancario/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimientoBancario = await _context.MovimientoBancarios
                .Include(m => m.Asiento)
                .Include(m => m.CuentaBancaria)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movimientoBancario == null)
            {
                return NotFound();
            }

            return View(movimientoBancario);
        }

        // POST: MovimientoBancario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var movimientoBancario = await _context.MovimientoBancarios.FindAsync(id);
            if (movimientoBancario != null)
            {
                _context.MovimientoBancarios.Remove(movimientoBancario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovimientoBancarioExists(Guid id)
        {
            return _context.MovimientoBancarios.Any(e => e.Id == id);
        }
    }
}
