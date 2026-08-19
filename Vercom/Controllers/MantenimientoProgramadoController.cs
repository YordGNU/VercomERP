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
    public class MantenimientoProgramadoController : Controller
    {
        private readonly AppDbContext _context;

        public MantenimientoProgramadoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: MantenimientoProgramado
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.MantenimientoProgramados.Include(m => m.Equipo);
            return View(await appDbContext.ToListAsync());
        }

        // GET: MantenimientoProgramado/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mantenimientoProgramado = await _context.MantenimientoProgramados
                .Include(m => m.Equipo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mantenimientoProgramado == null)
            {
                return NotFound();
            }

            return View(mantenimientoProgramado);
        }

        // GET: MantenimientoProgramado/Create
        public IActionResult Create()
        {
            ViewData["EquipoId"] = new SelectList(_context.Equipos, "Id", "Id");
            return View();
        }

        // POST: MantenimientoProgramado/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EquipoId,FechaProgramada,FechaEjecutada,Tipo,Descripcion,Costo,ResponsableId,Estado")] MantenimientoProgramado mantenimientoProgramado)
        {
            if (ModelState.IsValid)
            {
                mantenimientoProgramado.Id = Guid.NewGuid();
                _context.Add(mantenimientoProgramado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EquipoId"] = new SelectList(_context.Equipos, "Id", "Id", mantenimientoProgramado.EquipoId);
            return View(mantenimientoProgramado);
        }

        // GET: MantenimientoProgramado/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mantenimientoProgramado = await _context.MantenimientoProgramados.FindAsync(id);
            if (mantenimientoProgramado == null)
            {
                return NotFound();
            }
            ViewData["EquipoId"] = new SelectList(_context.Equipos, "Id", "Id", mantenimientoProgramado.EquipoId);
            return View(mantenimientoProgramado);
        }

        // POST: MantenimientoProgramado/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EquipoId,FechaProgramada,FechaEjecutada,Tipo,Descripcion,Costo,ResponsableId,Estado")] MantenimientoProgramado mantenimientoProgramado)
        {
            if (id != mantenimientoProgramado.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mantenimientoProgramado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MantenimientoProgramadoExists(mantenimientoProgramado.Id))
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
            ViewData["EquipoId"] = new SelectList(_context.Equipos, "Id", "Id", mantenimientoProgramado.EquipoId);
            return View(mantenimientoProgramado);
        }

        // GET: MantenimientoProgramado/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mantenimientoProgramado = await _context.MantenimientoProgramados
                .Include(m => m.Equipo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mantenimientoProgramado == null)
            {
                return NotFound();
            }

            return View(mantenimientoProgramado);
        }

        // POST: MantenimientoProgramado/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var mantenimientoProgramado = await _context.MantenimientoProgramados.FindAsync(id);
            if (mantenimientoProgramado != null)
            {
                _context.MantenimientoProgramados.Remove(mantenimientoProgramado);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MantenimientoProgramadoExists(Guid id)
        {
            return _context.MantenimientoProgramados.Any(e => e.Id == id);
        }
    }
}
