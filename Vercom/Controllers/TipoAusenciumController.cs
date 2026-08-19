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
    public class TipoAusenciumController : Controller
    {
        private readonly AppDbContext _context;

        public TipoAusenciumController(AppDbContext context)
        {
            _context = context;
        }

        // GET: TipoAusencium
        public async Task<IActionResult> Index()
        {
            return View(await _context.TipoAusencia.ToListAsync());
        }

        // GET: TipoAusencium/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoAusencium = await _context.TipoAusencia
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoAusencium == null)
            {
                return NotFound();
            }

            return View(tipoAusencium);
        }

        // GET: TipoAusencium/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TipoAusencium/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Codigo,Nombre,Remunerada,AfectaVacaciones")] TipoAusencium tipoAusencium)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tipoAusencium);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tipoAusencium);
        }

        // GET: TipoAusencium/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoAusencium = await _context.TipoAusencia.FindAsync(id);
            if (tipoAusencium == null)
            {
                return NotFound();
            }
            return View(tipoAusencium);
        }

        // POST: TipoAusencium/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nombre,Remunerada,AfectaVacaciones")] TipoAusencium tipoAusencium)
        {
            if (id != tipoAusencium.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoAusencium);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoAusenciumExists(tipoAusencium.Id))
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
            return View(tipoAusencium);
        }

        // GET: TipoAusencium/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoAusencium = await _context.TipoAusencia
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoAusencium == null)
            {
                return NotFound();
            }

            return View(tipoAusencium);
        }

        // POST: TipoAusencium/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tipoAusencium = await _context.TipoAusencia.FindAsync(id);
            if (tipoAusencium != null)
            {
                _context.TipoAusencia.Remove(tipoAusencium);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipoAusenciumExists(int id)
        {
            return _context.TipoAusencia.Any(e => e.Id == id);
        }
    }
}
