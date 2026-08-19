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
    public class TopePrecioMfpController : Controller
    {
        private readonly AppDbContext _context;

        public TopePrecioMfpController(AppDbContext context)
        {
            _context = context;
        }

        // GET: TopePrecioMfp
        public async Task<IActionResult> Index()
        {
            return View(await _context.TopePrecioMfps.ToListAsync());
        }

        // GET: TopePrecioMfp/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topePrecioMfp = await _context.TopePrecioMfps
                .FirstOrDefaultAsync(m => m.Id == id);
            if (topePrecioMfp == null)
            {
                return NotFound();
            }

            return View(topePrecioMfp);
        }

        // GET: TopePrecioMfp/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TopePrecioMfp/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductoId,FamiliaId,PrecioMaximo,VigenteDesde,VigenteHasta,ResolucionReferencia")] TopePrecioMfp topePrecioMfp)
        {
            if (ModelState.IsValid)
            {
                topePrecioMfp.Id = Guid.NewGuid();
                _context.Add(topePrecioMfp);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(topePrecioMfp);
        }

        // GET: TopePrecioMfp/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topePrecioMfp = await _context.TopePrecioMfps.FindAsync(id);
            if (topePrecioMfp == null)
            {
                return NotFound();
            }
            return View(topePrecioMfp);
        }

        // POST: TopePrecioMfp/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ProductoId,FamiliaId,PrecioMaximo,VigenteDesde,VigenteHasta,ResolucionReferencia")] TopePrecioMfp topePrecioMfp)
        {
            if (id != topePrecioMfp.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(topePrecioMfp);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TopePrecioMfpExists(topePrecioMfp.Id))
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
            return View(topePrecioMfp);
        }

        // GET: TopePrecioMfp/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topePrecioMfp = await _context.TopePrecioMfps
                .FirstOrDefaultAsync(m => m.Id == id);
            if (topePrecioMfp == null)
            {
                return NotFound();
            }

            return View(topePrecioMfp);
        }

        // POST: TopePrecioMfp/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var topePrecioMfp = await _context.TopePrecioMfps.FindAsync(id);
            if (topePrecioMfp != null)
            {
                _context.TopePrecioMfps.Remove(topePrecioMfp);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TopePrecioMfpExists(Guid id)
        {
            return _context.TopePrecioMfps.Any(e => e.Id == id);
        }
    }
}
