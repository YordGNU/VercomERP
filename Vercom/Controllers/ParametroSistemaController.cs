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
    public class ParametroSistemaController : Controller
    {
        private readonly AppDbContext _context;

        public ParametroSistemaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ParametroSistema
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ParametroSistemas.Include(p => p.Entidad);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ParametroSistema/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parametroSistema = await _context.ParametroSistemas
                .Include(p => p.Entidad)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parametroSistema == null)
            {
                return NotFound();
            }

            return View(parametroSistema);
        }

        // GET: ParametroSistema/Create
        public IActionResult Create()
        {
            ViewData["EntidadId"] = new SelectList(_context.Entidads, "Id", "Id");
            return View();
        }

        // POST: ParametroSistema/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,Codigo,Valor,TipoDato,Descripcion,VigenteDesde,VigenteHasta")] ParametroSistema parametroSistema)
        {
            if (ModelState.IsValid)
            {
                _context.Add(parametroSistema);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EntidadId"] = new SelectList(_context.Entidads, "Id", "Id", parametroSistema.EntidadId);
            return View(parametroSistema);
        }

        // GET: ParametroSistema/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parametroSistema = await _context.ParametroSistemas.FindAsync(id);
            if (parametroSistema == null)
            {
                return NotFound();
            }
            ViewData["EntidadId"] = new SelectList(_context.Entidads, "Id", "Id", parametroSistema.EntidadId);
            return View(parametroSistema);
        }

        // POST: ParametroSistema/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EntidadId,Codigo,Valor,TipoDato,Descripcion,VigenteDesde,VigenteHasta")] ParametroSistema parametroSistema)
        {
            if (id != parametroSistema.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(parametroSistema);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParametroSistemaExists(parametroSistema.Id))
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
            ViewData["EntidadId"] = new SelectList(_context.Entidads, "Id", "Id", parametroSistema.EntidadId);
            return View(parametroSistema);
        }

        // GET: ParametroSistema/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parametroSistema = await _context.ParametroSistemas
                .Include(p => p.Entidad)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parametroSistema == null)
            {
                return NotFound();
            }

            return View(parametroSistema);
        }

        // POST: ParametroSistema/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var parametroSistema = await _context.ParametroSistemas.FindAsync(id);
            if (parametroSistema != null)
            {
                _context.ParametroSistemas.Remove(parametroSistema);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParametroSistemaExists(int id)
        {
            return _context.ParametroSistemas.Any(e => e.Id == id);
        }
    }
}
