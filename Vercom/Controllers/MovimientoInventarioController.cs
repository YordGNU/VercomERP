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
    public class MovimientoInventarioController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public MovimientoInventarioController(AppDbContext context)
        {
            _context = context;
        }

        // GET: MovimientoInventario
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.MovimientoInventarios.Include(m => m.AlmacenDestino).Include(m => m.AlmacenOrigen).Include(m => m.TipoMovimiento);
            return View(await appDbContext.ToListAsync());
        }

        // GET: MovimientoInventario/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimientoInventario = await _context.MovimientoInventarios
                .Include(m => m.AlmacenDestino)
                .Include(m => m.AlmacenOrigen)
                .Include(m => m.TipoMovimiento)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movimientoInventario == null)
            {
                return NotFound();
            }

            return View(movimientoInventario);
        }

        // GET: MovimientoInventario/Create
        public IActionResult Create()
        {
            ViewData["AlmacenDestinoId"] = new SelectList(_context.Almacens, "Id", "Id");
            ViewData["AlmacenOrigenId"] = new SelectList(_context.Almacens, "Id", "Id");
            ViewData["TipoMovimientoId"] = new SelectList(_context.TipoMovimientos, "Id", "Id");
            return View();
        }

        // POST: MovimientoInventario/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,TipoMovimientoId,NumeroDocumento,AlmacenOrigenId,AlmacenDestinoId,Fecha,ReferenciaExternaTipo,ReferenciaExternaId,Canal,DispositivoPosId,Observaciones,AsientoId,CreadoPor,CreadoEn")] MovimientoInventario movimientoInventario)
        {
            if (ModelState.IsValid)
            {
                movimientoInventario.Id = Guid.NewGuid();
                _context.Add(movimientoInventario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AlmacenDestinoId"] = new SelectList(_context.Almacens, "Id", "Id", movimientoInventario.AlmacenDestinoId);
            ViewData["AlmacenOrigenId"] = new SelectList(_context.Almacens, "Id", "Id", movimientoInventario.AlmacenOrigenId);
            ViewData["TipoMovimientoId"] = new SelectList(_context.TipoMovimientos, "Id", "Id", movimientoInventario.TipoMovimientoId);
            return View(movimientoInventario);
        }

        // GET: MovimientoInventario/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimientoInventario = await _context.MovimientoInventarios.FindAsync(id);
            if (movimientoInventario == null)
            {
                return NotFound();
            }
            ViewData["AlmacenDestinoId"] = new SelectList(_context.Almacens, "Id", "Id", movimientoInventario.AlmacenDestinoId);
            ViewData["AlmacenOrigenId"] = new SelectList(_context.Almacens, "Id", "Id", movimientoInventario.AlmacenOrigenId);
            ViewData["TipoMovimientoId"] = new SelectList(_context.TipoMovimientos, "Id", "Id", movimientoInventario.TipoMovimientoId);
            return View(movimientoInventario);
        }

        // POST: MovimientoInventario/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,TipoMovimientoId,NumeroDocumento,AlmacenOrigenId,AlmacenDestinoId,Fecha,ReferenciaExternaTipo,ReferenciaExternaId,Canal,DispositivoPosId,Observaciones,AsientoId,CreadoPor,CreadoEn")] MovimientoInventario movimientoInventario)
        {
            if (id != movimientoInventario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movimientoInventario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovimientoInventarioExists(movimientoInventario.Id))
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
            ViewData["AlmacenDestinoId"] = new SelectList(_context.Almacens, "Id", "Id", movimientoInventario.AlmacenDestinoId);
            ViewData["AlmacenOrigenId"] = new SelectList(_context.Almacens, "Id", "Id", movimientoInventario.AlmacenOrigenId);
            ViewData["TipoMovimientoId"] = new SelectList(_context.TipoMovimientos, "Id", "Id", movimientoInventario.TipoMovimientoId);
            return View(movimientoInventario);
        }

        // GET: MovimientoInventario/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimientoInventario = await _context.MovimientoInventarios
                .Include(m => m.AlmacenDestino)
                .Include(m => m.AlmacenOrigen)
                .Include(m => m.TipoMovimiento)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movimientoInventario == null)
            {
                return NotFound();
            }

            return View(movimientoInventario);
        }

        // POST: MovimientoInventario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var movimientoInventario = await _context.MovimientoInventarios.FindAsync(id);
            if (movimientoInventario != null)
            {
                _context.MovimientoInventarios.Remove(movimientoInventario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovimientoInventarioExists(Guid id)
        {
            return _context.MovimientoInventarios.Any(e => e.Id == id);
        }
    }
}
