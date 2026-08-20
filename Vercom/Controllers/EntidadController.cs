using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class EntidadController : Controller
    {
        private readonly AppDbContext _context;

        public EntidadController(AppDbContext context)
        {
            _context = context;
        }

        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        private bool IsMasterUser => User.Identity?.Name == "master";

        // GET: Entidad
        public async Task<IActionResult> Index()
        {
            if (!IsMasterUser)
            {
                return RedirectToAction(nameof(Details), new { id = CurrentEntidadId });
            }
            return View(await _context.Entidads.ToListAsync());
        }

        // GET: Entidad/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) id = CurrentEntidadId;

            if (!IsMasterUser && id != CurrentEntidadId)
            {
                return Forbid();
            }

            var entidad = await _context.Entidads.FirstOrDefaultAsync(m => m.Id == id);
            if (entidad == null) return NotFound();

            return View(entidad);
        }

        // GET: Entidad/Create
        public IActionResult Create()
        {
            if (!IsMasterUser) return Forbid();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Entidad entidad)
        {
            if (!IsMasterUser) return Forbid();

            if (ModelState.IsValid)
            {
                entidad.Id = Guid.NewGuid();
                entidad.CreadoEn = DateTimeOffset.Now;
                entidad.ActualizadoEn = DateTimeOffset.Now;
                _context.Add(entidad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(entidad);
        }

        // GET: Entidad/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) id = CurrentEntidadId;

            if (!IsMasterUser && id != CurrentEntidadId) return Forbid();

            var entidad = await _context.Entidads.FindAsync(id);
            if (entidad == null) return NotFound();
            return View(entidad);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Entidad entidad)
        {
            if (id != entidad.Id) return NotFound();
            if (!IsMasterUser && id != CurrentEntidadId) return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    entidad.ActualizadoEn = DateTimeOffset.Now;
                    _context.Update(entidad);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntidadExists(entidad.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Details), new { id = entidad.Id });
            }
            return View(entidad);
        }

        // GET: Entidad/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (!IsMasterUser) return Forbid();
            if (id == null) return NotFound();

            var entidad = await _context.Entidads.FirstOrDefaultAsync(m => m.Id == id);
            if (entidad == null) return NotFound();

            return View(entidad);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (!IsMasterUser) return Forbid();

            var entidad = await _context.Entidads.FindAsync(id);
            if (entidad != null)
            {
                _context.Entidads.Remove(entidad);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EntidadExists(Guid id)
        {
            return _context.Entidads.Any(e => e.Id == id);
        }
    }
}
