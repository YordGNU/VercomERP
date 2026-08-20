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
    public class CentroCostoController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public CentroCostoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CentroCosto
        public async Task<IActionResult> Index()
        {
            return View(await _context.CentroCostos.ToListAsync());
        }

        // GET: CentroCosto/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var centroCosto = await _context.CentroCostos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (centroCosto == null)
            {
                return NotFound();
            }

            return View(centroCosto);
        }

        // GET: CentroCosto/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CentroCosto/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,Codigo,Nombre,SucursalId,Activo")] CentroCosto centroCosto)
        {
            if (ModelState.IsValid)
            {
                centroCosto.Id = Guid.NewGuid();
                _context.Add(centroCosto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(centroCosto);
        }

        // GET: CentroCosto/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var centroCosto = await _context.CentroCostos.FindAsync(id);
            if (centroCosto == null)
            {
                return NotFound();
            }
            return View(centroCosto);
        }

        // POST: CentroCosto/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,Codigo,Nombre,SucursalId,Activo")] CentroCosto centroCosto)
        {
            if (id != centroCosto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(centroCosto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CentroCostoExists(centroCosto.Id))
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
            return View(centroCosto);
        }

        // GET: CentroCosto/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var centroCosto = await _context.CentroCostos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (centroCosto == null)
            {
                return NotFound();
            }

            return View(centroCosto);
        }

        // POST: CentroCosto/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var centroCosto = await _context.CentroCostos.FindAsync(id);
            if (centroCosto != null)
            {
                _context.CentroCostos.Remove(centroCosto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CentroCostoExists(Guid id)
        {
            return _context.CentroCostos.Any(e => e.Id == id);
        }
    }
}
