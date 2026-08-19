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
    public class TipoObligacionFiscalController : Controller
    {
        private readonly AppDbContext _context;

        public TipoObligacionFiscalController(AppDbContext context)
        {
            _context = context;
        }

        // GET: TipoObligacionFiscal
        public async Task<IActionResult> Index()
        {
            return View(await _context.TipoObligacionFiscals.ToListAsync());
        }

        // GET: TipoObligacionFiscal/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoObligacionFiscal = await _context.TipoObligacionFiscals
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoObligacionFiscal == null)
            {
                return NotFound();
            }

            return View(tipoObligacionFiscal);
        }

        // GET: TipoObligacionFiscal/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TipoObligacionFiscal/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Codigo,Nombre,Periodicidad,TasaActual,BaseLegal")] TipoObligacionFiscal tipoObligacionFiscal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tipoObligacionFiscal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tipoObligacionFiscal);
        }

        // GET: TipoObligacionFiscal/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoObligacionFiscal = await _context.TipoObligacionFiscals.FindAsync(id);
            if (tipoObligacionFiscal == null)
            {
                return NotFound();
            }
            return View(tipoObligacionFiscal);
        }

        // POST: TipoObligacionFiscal/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nombre,Periodicidad,TasaActual,BaseLegal")] TipoObligacionFiscal tipoObligacionFiscal)
        {
            if (id != tipoObligacionFiscal.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoObligacionFiscal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoObligacionFiscalExists(tipoObligacionFiscal.Id))
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
            return View(tipoObligacionFiscal);
        }

        // GET: TipoObligacionFiscal/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoObligacionFiscal = await _context.TipoObligacionFiscals
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoObligacionFiscal == null)
            {
                return NotFound();
            }

            return View(tipoObligacionFiscal);
        }

        // POST: TipoObligacionFiscal/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tipoObligacionFiscal = await _context.TipoObligacionFiscals.FindAsync(id);
            if (tipoObligacionFiscal != null)
            {
                _context.TipoObligacionFiscals.Remove(tipoObligacionFiscal);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipoObligacionFiscalExists(int id)
        {
            return _context.TipoObligacionFiscals.Any(e => e.Id == id);
        }
    }
}
