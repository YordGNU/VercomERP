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
    public class FamiliaProductoController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public FamiliaProductoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: FamiliaProducto
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.FamiliaProductos.Include(f => f.FamiliaPadre);
            return View(await appDbContext.ToListAsync());
        }

        // GET: FamiliaProducto/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var familiaProducto = await _context.FamiliaProductos
                .Include(f => f.FamiliaPadre)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (familiaProducto == null)
            {
                return NotFound();
            }

            return View(familiaProducto);
        }

        // GET: FamiliaProducto/Create
        public IActionResult Create()
        {
            ViewData["FamiliaPadreId"] = new SelectList(_context.FamiliaProductos, "Id", "Id");
            return View();
        }

        // POST: FamiliaProducto/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,Codigo,Nombre,FamiliaPadreId")] FamiliaProducto familiaProducto)
        {
            if (ModelState.IsValid)
            {
                familiaProducto.Id = Guid.NewGuid();
                _context.Add(familiaProducto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FamiliaPadreId"] = new SelectList(_context.FamiliaProductos, "Id", "Id", familiaProducto.FamiliaPadreId);
            return View(familiaProducto);
        }

        // GET: FamiliaProducto/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var familiaProducto = await _context.FamiliaProductos.FindAsync(id);
            if (familiaProducto == null)
            {
                return NotFound();
            }
            ViewData["FamiliaPadreId"] = new SelectList(_context.FamiliaProductos, "Id", "Id", familiaProducto.FamiliaPadreId);
            return View(familiaProducto);
        }

        // POST: FamiliaProducto/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,Codigo,Nombre,FamiliaPadreId")] FamiliaProducto familiaProducto)
        {
            if (id != familiaProducto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(familiaProducto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FamiliaProductoExists(familiaProducto.Id))
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
            ViewData["FamiliaPadreId"] = new SelectList(_context.FamiliaProductos, "Id", "Id", familiaProducto.FamiliaPadreId);
            return View(familiaProducto);
        }

        // GET: FamiliaProducto/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var familiaProducto = await _context.FamiliaProductos
                .Include(f => f.FamiliaPadre)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (familiaProducto == null)
            {
                return NotFound();
            }

            return View(familiaProducto);
        }

        // POST: FamiliaProducto/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var familiaProducto = await _context.FamiliaProductos.FindAsync(id);
            if (familiaProducto != null)
            {
                _context.FamiliaProductos.Remove(familiaProducto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FamiliaProductoExists(Guid id)
        {
            return _context.FamiliaProductos.Any(e => e.Id == id);
        }
    }
}
