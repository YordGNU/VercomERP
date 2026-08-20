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
    public class IndicadorValorController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public IndicadorValorController(AppDbContext context)
        {
            _context = context;
        }

        // GET: IndicadorValor
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.IndicadorValors.Include(i => i.Indicador).Include(i => i.Periodo);
            return View(await appDbContext.ToListAsync());
        }

        // GET: IndicadorValor/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var indicadorValor = await _context.IndicadorValors
                .Include(i => i.Indicador)
                .Include(i => i.Periodo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (indicadorValor == null)
            {
                return NotFound();
            }

            return View(indicadorValor);
        }

        // GET: IndicadorValor/Create
        public IActionResult Create()
        {
            ViewData["IndicadorId"] = new SelectList(_context.Indicadors, "Id", "Id");
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id");
            return View();
        }

        // POST: IndicadorValor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,IndicadorId,PeriodoId,Valor,CalculadoEn")] IndicadorValor indicadorValor)
        {
            if (ModelState.IsValid)
            {
                indicadorValor.Id = Guid.NewGuid();
                _context.Add(indicadorValor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IndicadorId"] = new SelectList(_context.Indicadors, "Id", "Id", indicadorValor.IndicadorId);
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id", indicadorValor.PeriodoId);
            return View(indicadorValor);
        }

        // GET: IndicadorValor/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var indicadorValor = await _context.IndicadorValors.FindAsync(id);
            if (indicadorValor == null)
            {
                return NotFound();
            }
            ViewData["IndicadorId"] = new SelectList(_context.Indicadors, "Id", "Id", indicadorValor.IndicadorId);
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id", indicadorValor.PeriodoId);
            return View(indicadorValor);
        }

        // POST: IndicadorValor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,IndicadorId,PeriodoId,Valor,CalculadoEn")] IndicadorValor indicadorValor)
        {
            if (id != indicadorValor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(indicadorValor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IndicadorValorExists(indicadorValor.Id))
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
            ViewData["IndicadorId"] = new SelectList(_context.Indicadors, "Id", "Id", indicadorValor.IndicadorId);
            ViewData["PeriodoId"] = new SelectList(_context.PeriodoContables, "Id", "Id", indicadorValor.PeriodoId);
            return View(indicadorValor);
        }

        // GET: IndicadorValor/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var indicadorValor = await _context.IndicadorValors
                .Include(i => i.Indicador)
                .Include(i => i.Periodo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (indicadorValor == null)
            {
                return NotFound();
            }

            return View(indicadorValor);
        }

        // POST: IndicadorValor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var indicadorValor = await _context.IndicadorValors.FindAsync(id);
            if (indicadorValor != null)
            {
                _context.IndicadorValors.Remove(indicadorValor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IndicadorValorExists(Guid id)
        {
            return _context.IndicadorValors.Any(e => e.Id == id);
        }
    }
}
