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
    public class ListaMaterialesDetalleController : Controller
    {
        private readonly AppDbContext _context;

        public ListaMaterialesDetalleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ListaMaterialesDetalle
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ListaMaterialesDetalles.Include(l => l.ListaMateriales);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ListaMaterialesDetalle/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listaMaterialesDetalle = await _context.ListaMaterialesDetalles
                .Include(l => l.ListaMateriales)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (listaMaterialesDetalle == null)
            {
                return NotFound();
            }

            return View(listaMaterialesDetalle);
        }

        // GET: ListaMaterialesDetalle/Create
        public IActionResult Create()
        {
            ViewData["ListaMaterialesId"] = new SelectList(_context.ListaMateriales, "Id", "Id");
            return View();
        }

        // POST: ListaMaterialesDetalle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ListaMaterialesId,ProductoInsumoId,CantidadRequerida,PorcentajeMerma")] ListaMaterialesDetalle listaMaterialesDetalle)
        {
            if (ModelState.IsValid)
            {
                listaMaterialesDetalle.Id = Guid.NewGuid();
                _context.Add(listaMaterialesDetalle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ListaMaterialesId"] = new SelectList(_context.ListaMateriales, "Id", "Id", listaMaterialesDetalle.ListaMaterialesId);
            return View(listaMaterialesDetalle);
        }

        // GET: ListaMaterialesDetalle/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listaMaterialesDetalle = await _context.ListaMaterialesDetalles.FindAsync(id);
            if (listaMaterialesDetalle == null)
            {
                return NotFound();
            }
            ViewData["ListaMaterialesId"] = new SelectList(_context.ListaMateriales, "Id", "Id", listaMaterialesDetalle.ListaMaterialesId);
            return View(listaMaterialesDetalle);
        }

        // POST: ListaMaterialesDetalle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ListaMaterialesId,ProductoInsumoId,CantidadRequerida,PorcentajeMerma")] ListaMaterialesDetalle listaMaterialesDetalle)
        {
            if (id != listaMaterialesDetalle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listaMaterialesDetalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListaMaterialesDetalleExists(listaMaterialesDetalle.Id))
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
            ViewData["ListaMaterialesId"] = new SelectList(_context.ListaMateriales, "Id", "Id", listaMaterialesDetalle.ListaMaterialesId);
            return View(listaMaterialesDetalle);
        }

        // GET: ListaMaterialesDetalle/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listaMaterialesDetalle = await _context.ListaMaterialesDetalles
                .Include(l => l.ListaMateriales)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (listaMaterialesDetalle == null)
            {
                return NotFound();
            }

            return View(listaMaterialesDetalle);
        }

        // POST: ListaMaterialesDetalle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var listaMaterialesDetalle = await _context.ListaMaterialesDetalles.FindAsync(id);
            if (listaMaterialesDetalle != null)
            {
                _context.ListaMaterialesDetalles.Remove(listaMaterialesDetalle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ListaMaterialesDetalleExists(Guid id)
        {
            return _context.ListaMaterialesDetalles.Any(e => e.Id == id);
        }
    }
}
