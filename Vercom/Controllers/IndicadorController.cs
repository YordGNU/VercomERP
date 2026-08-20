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
    public class IndicadorController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public IndicadorController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Indicador
        public async Task<IActionResult> Index()
        {
            return View(await _context.Indicadors.ToListAsync());
        }

        // GET: Indicador/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var indicador = await _context.Indicadors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (indicador == null)
            {
                return NotFound();
            }

            return View(indicador);
        }

        // GET: Indicador/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Indicador/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Codigo,Nombre,Categoria,FormulaDescripcion")] Indicador indicador)
        {
            if (ModelState.IsValid)
            {
                _context.Add(indicador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(indicador);
        }

        // GET: Indicador/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var indicador = await _context.Indicadors.FindAsync(id);
            if (indicador == null)
            {
                return NotFound();
            }
            return View(indicador);
        }

        // POST: Indicador/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nombre,Categoria,FormulaDescripcion")] Indicador indicador)
        {
            if (id != indicador.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(indicador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IndicadorExists(indicador.Id))
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
            return View(indicador);
        }

        // GET: Indicador/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var indicador = await _context.Indicadors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (indicador == null)
            {
                return NotFound();
            }

            return View(indicador);
        }

        // POST: Indicador/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var indicador = await _context.Indicadors.FindAsync(id);
            if (indicador != null)
            {
                _context.Indicadors.Remove(indicador);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IndicadorExists(int id)
        {
            return _context.Indicadors.Any(e => e.Id == id);
        }
    }
}
