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
    public class MermaController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public MermaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Merma
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Mermas.Include(m => m.OrdenProduccion);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Merma/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var merma = await _context.Mermas
                .Include(m => m.OrdenProduccion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (merma == null)
            {
                return NotFound();
            }

            return View(merma);
        }

        // GET: Merma/Create
        public IActionResult Create()
        {
            ViewData["OrdenProduccionId"] = new SelectList(_context.OrdenProduccions, "Id", "Id");
            return View();
        }

        // POST: Merma/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrdenProduccionId,ProductoId,Cantidad,Causa,ValorContable,AsientoId,Fecha,Observaciones")] Merma merma)
        {
            if (ModelState.IsValid)
            {
                merma.Id = Guid.NewGuid();
                _context.Add(merma);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrdenProduccionId"] = new SelectList(_context.OrdenProduccions, "Id", "Id", merma.OrdenProduccionId);
            return View(merma);
        }

        // GET: Merma/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var merma = await _context.Mermas.FindAsync(id);
            if (merma == null)
            {
                return NotFound();
            }
            ViewData["OrdenProduccionId"] = new SelectList(_context.OrdenProduccions, "Id", "Id", merma.OrdenProduccionId);
            return View(merma);
        }

        // POST: Merma/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,OrdenProduccionId,ProductoId,Cantidad,Causa,ValorContable,AsientoId,Fecha,Observaciones")] Merma merma)
        {
            if (id != merma.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(merma);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MermaExists(merma.Id))
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
            ViewData["OrdenProduccionId"] = new SelectList(_context.OrdenProduccions, "Id", "Id", merma.OrdenProduccionId);
            return View(merma);
        }

        // GET: Merma/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var merma = await _context.Mermas
                .Include(m => m.OrdenProduccion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (merma == null)
            {
                return NotFound();
            }

            return View(merma);
        }

        // POST: Merma/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var merma = await _context.Mermas.FindAsync(id);
            if (merma != null)
            {
                _context.Mermas.Remove(merma);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MermaExists(Guid id)
        {
            return _context.Mermas.Any(e => e.Id == id);
        }
    }
}
