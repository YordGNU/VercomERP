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
    public class ConteoFisicoDetalleController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public ConteoFisicoDetalleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ConteoFisicoDetalle
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ConteoFisicoDetalles.Include(c => c.Conteo).Include(c => c.MovimientoAjuste).Include(c => c.Producto);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ConteoFisicoDetalle/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conteoFisicoDetalle = await _context.ConteoFisicoDetalles
                .Include(c => c.Conteo)
                .Include(c => c.MovimientoAjuste)
                .Include(c => c.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (conteoFisicoDetalle == null)
            {
                return NotFound();
            }

            return View(conteoFisicoDetalle);
        }

        // GET: ConteoFisicoDetalle/Create
        public IActionResult Create()
        {
            ViewData["ConteoId"] = new SelectList(_context.ConteoFisicos, "Id", "Id");
            ViewData["MovimientoAjusteId"] = new SelectList(_context.MovimientoInventarios, "Id", "Id");
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id");
            return View();
        }

        // POST: ConteoFisicoDetalle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ConteoId,ProductoId,CantidadSistema,CantidadFisica,Diferencia,Justificacion,MovimientoAjusteId")] ConteoFisicoDetalle conteoFisicoDetalle)
        {
            if (ModelState.IsValid)
            {
                conteoFisicoDetalle.Id = Guid.NewGuid();
                _context.Add(conteoFisicoDetalle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConteoId"] = new SelectList(_context.ConteoFisicos, "Id", "Id", conteoFisicoDetalle.ConteoId);
            ViewData["MovimientoAjusteId"] = new SelectList(_context.MovimientoInventarios, "Id", "Id", conteoFisicoDetalle.MovimientoAjusteId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id", conteoFisicoDetalle.ProductoId);
            return View(conteoFisicoDetalle);
        }

        // GET: ConteoFisicoDetalle/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conteoFisicoDetalle = await _context.ConteoFisicoDetalles.FindAsync(id);
            if (conteoFisicoDetalle == null)
            {
                return NotFound();
            }
            ViewData["ConteoId"] = new SelectList(_context.ConteoFisicos, "Id", "Id", conteoFisicoDetalle.ConteoId);
            ViewData["MovimientoAjusteId"] = new SelectList(_context.MovimientoInventarios, "Id", "Id", conteoFisicoDetalle.MovimientoAjusteId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id", conteoFisicoDetalle.ProductoId);
            return View(conteoFisicoDetalle);
        }

        // POST: ConteoFisicoDetalle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ConteoId,ProductoId,CantidadSistema,CantidadFisica,Diferencia,Justificacion,MovimientoAjusteId")] ConteoFisicoDetalle conteoFisicoDetalle)
        {
            if (id != conteoFisicoDetalle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(conteoFisicoDetalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConteoFisicoDetalleExists(conteoFisicoDetalle.Id))
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
            ViewData["ConteoId"] = new SelectList(_context.ConteoFisicos, "Id", "Id", conteoFisicoDetalle.ConteoId);
            ViewData["MovimientoAjusteId"] = new SelectList(_context.MovimientoInventarios, "Id", "Id", conteoFisicoDetalle.MovimientoAjusteId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id", conteoFisicoDetalle.ProductoId);
            return View(conteoFisicoDetalle);
        }

        // GET: ConteoFisicoDetalle/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conteoFisicoDetalle = await _context.ConteoFisicoDetalles
                .Include(c => c.Conteo)
                .Include(c => c.MovimientoAjuste)
                .Include(c => c.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (conteoFisicoDetalle == null)
            {
                return NotFound();
            }

            return View(conteoFisicoDetalle);
        }

        // POST: ConteoFisicoDetalle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var conteoFisicoDetalle = await _context.ConteoFisicoDetalles.FindAsync(id);
            if (conteoFisicoDetalle != null)
            {
                _context.ConteoFisicoDetalles.Remove(conteoFisicoDetalle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConteoFisicoDetalleExists(Guid id)
        {
            return _context.ConteoFisicoDetalles.Any(e => e.Id == id);
        }
    }
}
