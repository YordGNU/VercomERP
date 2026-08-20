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
    public class NominaDetalleConceptoController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public NominaDetalleConceptoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: NominaDetalleConcepto
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.NominaDetalleConceptos.Include(n => n.Concepto).Include(n => n.NominaDetalle);
            return View(await appDbContext.ToListAsync());
        }

        // GET: NominaDetalleConcepto/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nominaDetalleConcepto = await _context.NominaDetalleConceptos
                .Include(n => n.Concepto)
                .Include(n => n.NominaDetalle)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nominaDetalleConcepto == null)
            {
                return NotFound();
            }

            return View(nominaDetalleConcepto);
        }

        // GET: NominaDetalleConcepto/Create
        public IActionResult Create()
        {
            ViewData["ConceptoId"] = new SelectList(_context.ConceptoNominas, "Id", "Id");
            ViewData["NominaDetalleId"] = new SelectList(_context.NominaDetalles, "Id", "Id");
            return View();
        }

        // POST: NominaDetalleConcepto/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NominaDetalleId,ConceptoId,Monto")] NominaDetalleConcepto nominaDetalleConcepto)
        {
            if (ModelState.IsValid)
            {
                nominaDetalleConcepto.Id = Guid.NewGuid();
                _context.Add(nominaDetalleConcepto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConceptoId"] = new SelectList(_context.ConceptoNominas, "Id", "Id", nominaDetalleConcepto.ConceptoId);
            ViewData["NominaDetalleId"] = new SelectList(_context.NominaDetalles, "Id", "Id", nominaDetalleConcepto.NominaDetalleId);
            return View(nominaDetalleConcepto);
        }

        // GET: NominaDetalleConcepto/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nominaDetalleConcepto = await _context.NominaDetalleConceptos.FindAsync(id);
            if (nominaDetalleConcepto == null)
            {
                return NotFound();
            }
            ViewData["ConceptoId"] = new SelectList(_context.ConceptoNominas, "Id", "Id", nominaDetalleConcepto.ConceptoId);
            ViewData["NominaDetalleId"] = new SelectList(_context.NominaDetalles, "Id", "Id", nominaDetalleConcepto.NominaDetalleId);
            return View(nominaDetalleConcepto);
        }

        // POST: NominaDetalleConcepto/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,NominaDetalleId,ConceptoId,Monto")] NominaDetalleConcepto nominaDetalleConcepto)
        {
            if (id != nominaDetalleConcepto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nominaDetalleConcepto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NominaDetalleConceptoExists(nominaDetalleConcepto.Id))
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
            ViewData["ConceptoId"] = new SelectList(_context.ConceptoNominas, "Id", "Id", nominaDetalleConcepto.ConceptoId);
            ViewData["NominaDetalleId"] = new SelectList(_context.NominaDetalles, "Id", "Id", nominaDetalleConcepto.NominaDetalleId);
            return View(nominaDetalleConcepto);
        }

        // GET: NominaDetalleConcepto/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nominaDetalleConcepto = await _context.NominaDetalleConceptos
                .Include(n => n.Concepto)
                .Include(n => n.NominaDetalle)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nominaDetalleConcepto == null)
            {
                return NotFound();
            }

            return View(nominaDetalleConcepto);
        }

        // POST: NominaDetalleConcepto/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var nominaDetalleConcepto = await _context.NominaDetalleConceptos.FindAsync(id);
            if (nominaDetalleConcepto != null)
            {
                _context.NominaDetalleConceptos.Remove(nominaDetalleConcepto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NominaDetalleConceptoExists(Guid id)
        {
            return _context.NominaDetalleConceptos.Any(e => e.Id == id);
        }
    }
}
