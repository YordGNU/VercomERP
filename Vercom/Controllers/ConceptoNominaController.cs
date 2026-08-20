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
    public class ConceptoNominaController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public ConceptoNominaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ConceptoNomina
        public async Task<IActionResult> Index()
        {
            return View(await _context.ConceptoNominas.ToListAsync());
        }

        // GET: ConceptoNomina/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conceptoNomina = await _context.ConceptoNominas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (conceptoNomina == null)
            {
                return NotFound();
            }

            return View(conceptoNomina);
        }

        // GET: ConceptoNomina/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ConceptoNomina/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Codigo,Nombre,Tipo,CuentaContableId,Formula")] ConceptoNomina conceptoNomina)
        {
            if (ModelState.IsValid)
            {
                _context.Add(conceptoNomina);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(conceptoNomina);
        }

        // GET: ConceptoNomina/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conceptoNomina = await _context.ConceptoNominas.FindAsync(id);
            if (conceptoNomina == null)
            {
                return NotFound();
            }
            return View(conceptoNomina);
        }

        // POST: ConceptoNomina/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nombre,Tipo,CuentaContableId,Formula")] ConceptoNomina conceptoNomina)
        {
            if (id != conceptoNomina.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(conceptoNomina);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConceptoNominaExists(conceptoNomina.Id))
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
            return View(conceptoNomina);
        }

        // GET: ConceptoNomina/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conceptoNomina = await _context.ConceptoNominas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (conceptoNomina == null)
            {
                return NotFound();
            }

            return View(conceptoNomina);
        }

        // POST: ConceptoNomina/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var conceptoNomina = await _context.ConceptoNominas.FindAsync(id);
            if (conceptoNomina != null)
            {
                _context.ConceptoNominas.Remove(conceptoNomina);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConceptoNominaExists(int id)
        {
            return _context.ConceptoNominas.Any(e => e.Id == id);
        }
    }
}
