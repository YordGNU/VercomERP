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
    public class ExistenciumController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public ExistenciumController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Existencium
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Existencia.Include(e => e.Almacen).Include(e => e.Producto);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Existencium/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var existencium = await _context.Existencia
                .Include(e => e.Almacen)
                .Include(e => e.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (existencium == null)
            {
                return NotFound();
            }

            return View(existencium);
        }

        // GET: Existencium/Create
        public IActionResult Create()
        {
            ViewData["AlmacenId"] = new SelectList(_context.Almacens, "Id", "Id");
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id");
            return View();
        }

        // POST: Existencium/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AlmacenId,ProductoId,Cantidad,CostoPromedio,StockMinimo,StockMaximo,ActualizadoEn")] Existencium existencium)
        {
            if (ModelState.IsValid)
            {
                existencium.Id = Guid.NewGuid();
                _context.Add(existencium);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AlmacenId"] = new SelectList(_context.Almacens, "Id", "Id", existencium.AlmacenId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id", existencium.ProductoId);
            return View(existencium);
        }

        // GET: Existencium/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var existencium = await _context.Existencia.FindAsync(id);
            if (existencium == null)
            {
                return NotFound();
            }
            ViewData["AlmacenId"] = new SelectList(_context.Almacens, "Id", "Id", existencium.AlmacenId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id", existencium.ProductoId);
            return View(existencium);
        }

        // POST: Existencium/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,AlmacenId,ProductoId,Cantidad,CostoPromedio,StockMinimo,StockMaximo,ActualizadoEn")] Existencium existencium)
        {
            if (id != existencium.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(existencium);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExistenciumExists(existencium.Id))
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
            ViewData["AlmacenId"] = new SelectList(_context.Almacens, "Id", "Id", existencium.AlmacenId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id", existencium.ProductoId);
            return View(existencium);
        }

        // GET: Existencium/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var existencium = await _context.Existencia
                .Include(e => e.Almacen)
                .Include(e => e.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (existencium == null)
            {
                return NotFound();
            }

            return View(existencium);
        }

        // POST: Existencium/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var existencium = await _context.Existencia.FindAsync(id);
            if (existencium != null)
            {
                _context.Existencia.Remove(existencium);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExistenciumExists(Guid id)
        {
            return _context.Existencia.Any(e => e.Id == id);
        }
    }
}
