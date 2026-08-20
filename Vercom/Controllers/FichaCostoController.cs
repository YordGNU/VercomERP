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
    public class FichaCostoController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public FichaCostoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: FichaCosto
        public async Task<IActionResult> Index()
        {
            return View(await _context.FichaCostos.ToListAsync());
        }

        // GET: FichaCosto/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fichaCosto = await _context.FichaCostos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fichaCosto == null)
            {
                return NotFound();
            }

            return View(fichaCosto);
        }

        // GET: FichaCosto/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FichaCosto/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,ProductoId,Version,VigenteDesde,VigenteHasta,CostoMateriaPrima,CostoManoObra,GastosIndirectos,CostoTotalUnitario,MargenPorcentaje,PrecioSugerido,Estado,CreadoPor,CreadoEn")] FichaCosto fichaCosto)
        {
            if (ModelState.IsValid)
            {
                fichaCosto.Id = Guid.NewGuid();
                _context.Add(fichaCosto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(fichaCosto);
        }

        // GET: FichaCosto/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fichaCosto = await _context.FichaCostos.FindAsync(id);
            if (fichaCosto == null)
            {
                return NotFound();
            }
            return View(fichaCosto);
        }

        // POST: FichaCosto/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,ProductoId,Version,VigenteDesde,VigenteHasta,CostoMateriaPrima,CostoManoObra,GastosIndirectos,CostoTotalUnitario,MargenPorcentaje,PrecioSugerido,Estado,CreadoPor,CreadoEn")] FichaCosto fichaCosto)
        {
            if (id != fichaCosto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fichaCosto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FichaCostoExists(fichaCosto.Id))
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
            return View(fichaCosto);
        }

        // GET: FichaCosto/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fichaCosto = await _context.FichaCostos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fichaCosto == null)
            {
                return NotFound();
            }

            return View(fichaCosto);
        }

        // POST: FichaCosto/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var fichaCosto = await _context.FichaCostos.FindAsync(id);
            if (fichaCosto != null)
            {
                _context.FichaCostos.Remove(fichaCosto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FichaCostoExists(Guid id)
        {
            return _context.FichaCostos.Any(e => e.Id == id);
        }
    }
}
