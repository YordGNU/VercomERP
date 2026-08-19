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
    public class ListaPrecioDetalleController : Controller
    {
        private readonly AppDbContext _context;

        public ListaPrecioDetalleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ListaPrecioDetalle
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ListaPrecioDetalles.Include(l => l.ListaPrecio).Include(l => l.Producto);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ListaPrecioDetalle/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listaPrecioDetalle = await _context.ListaPrecioDetalles
                .Include(l => l.ListaPrecio)
                .Include(l => l.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (listaPrecioDetalle == null)
            {
                return NotFound();
            }

            return View(listaPrecioDetalle);
        }

        // GET: ListaPrecioDetalle/Create
        public IActionResult Create()
        {
            ViewData["ListaPrecioId"] = new SelectList(_context.ListaPrecios, "Id", "Id");
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id");
            return View();
        }

        // POST: ListaPrecioDetalle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ListaPrecioId,ProductoId,Precio")] ListaPrecioDetalle listaPrecioDetalle)
        {
            if (ModelState.IsValid)
            {
                listaPrecioDetalle.Id = Guid.NewGuid();
                _context.Add(listaPrecioDetalle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ListaPrecioId"] = new SelectList(_context.ListaPrecios, "Id", "Id", listaPrecioDetalle.ListaPrecioId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id", listaPrecioDetalle.ProductoId);
            return View(listaPrecioDetalle);
        }

        // GET: ListaPrecioDetalle/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listaPrecioDetalle = await _context.ListaPrecioDetalles.FindAsync(id);
            if (listaPrecioDetalle == null)
            {
                return NotFound();
            }
            ViewData["ListaPrecioId"] = new SelectList(_context.ListaPrecios, "Id", "Id", listaPrecioDetalle.ListaPrecioId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id", listaPrecioDetalle.ProductoId);
            return View(listaPrecioDetalle);
        }

        // POST: ListaPrecioDetalle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ListaPrecioId,ProductoId,Precio")] ListaPrecioDetalle listaPrecioDetalle)
        {
            if (id != listaPrecioDetalle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listaPrecioDetalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListaPrecioDetalleExists(listaPrecioDetalle.Id))
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
            ViewData["ListaPrecioId"] = new SelectList(_context.ListaPrecios, "Id", "Id", listaPrecioDetalle.ListaPrecioId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id", listaPrecioDetalle.ProductoId);
            return View(listaPrecioDetalle);
        }

        // GET: ListaPrecioDetalle/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listaPrecioDetalle = await _context.ListaPrecioDetalles
                .Include(l => l.ListaPrecio)
                .Include(l => l.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (listaPrecioDetalle == null)
            {
                return NotFound();
            }

            return View(listaPrecioDetalle);
        }

        // POST: ListaPrecioDetalle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var listaPrecioDetalle = await _context.ListaPrecioDetalles.FindAsync(id);
            if (listaPrecioDetalle != null)
            {
                _context.ListaPrecioDetalles.Remove(listaPrecioDetalle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ListaPrecioDetalleExists(Guid id)
        {
            return _context.ListaPrecioDetalles.Any(e => e.Id == id);
        }
    }
}
