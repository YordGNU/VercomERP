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
    public class ConsecutivoController : Controller
    {
        private readonly AppDbContext _context;

        public ConsecutivoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Consecutivo
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Consecutivos.Include(c => c.Entidad).Include(c => c.Sucursal);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Consecutivo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consecutivo = await _context.Consecutivos
                .Include(c => c.Entidad)
                .Include(c => c.Sucursal)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (consecutivo == null)
            {
                return NotFound();
            }

            return View(consecutivo);
        }

        // GET: Consecutivo/Create
        public IActionResult Create()
        {
            ViewData["EntidadId"] = new SelectList(_context.Entidads, "Id", "Id");
            ViewData["SucursalId"] = new SelectList(_context.Sucursals, "Id", "Id");
            return View();
        }

        // POST: Consecutivo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,SucursalId,TipoDocumento,Serie,UltimoNumero,LongitudPadding,ActualizadoEn")] Consecutivo consecutivo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(consecutivo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EntidadId"] = new SelectList(_context.Entidads, "Id", "Id", consecutivo.EntidadId);
            ViewData["SucursalId"] = new SelectList(_context.Sucursals, "Id", "Id", consecutivo.SucursalId);
            return View(consecutivo);
        }

        // GET: Consecutivo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consecutivo = await _context.Consecutivos.FindAsync(id);
            if (consecutivo == null)
            {
                return NotFound();
            }
            ViewData["EntidadId"] = new SelectList(_context.Entidads, "Id", "Id", consecutivo.EntidadId);
            ViewData["SucursalId"] = new SelectList(_context.Sucursals, "Id", "Id", consecutivo.SucursalId);
            return View(consecutivo);
        }

        // POST: Consecutivo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EntidadId,SucursalId,TipoDocumento,Serie,UltimoNumero,LongitudPadding,ActualizadoEn")] Consecutivo consecutivo)
        {
            if (id != consecutivo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(consecutivo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConsecutivoExists(consecutivo.Id))
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
            ViewData["EntidadId"] = new SelectList(_context.Entidads, "Id", "Id", consecutivo.EntidadId);
            ViewData["SucursalId"] = new SelectList(_context.Sucursals, "Id", "Id", consecutivo.SucursalId);
            return View(consecutivo);
        }

        // GET: Consecutivo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consecutivo = await _context.Consecutivos
                .Include(c => c.Entidad)
                .Include(c => c.Sucursal)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (consecutivo == null)
            {
                return NotFound();
            }

            return View(consecutivo);
        }

        // POST: Consecutivo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var consecutivo = await _context.Consecutivos.FindAsync(id);
            if (consecutivo != null)
            {
                _context.Consecutivos.Remove(consecutivo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConsecutivoExists(int id)
        {
            return _context.Consecutivos.Any(e => e.Id == id);
        }
    }
}
