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
    public class ConteoFisicoController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public ConteoFisicoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ConteoFisico
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ConteoFisicos.Include(c => c.Almacen);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ConteoFisico/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conteoFisico = await _context.ConteoFisicos
                .Include(c => c.Almacen)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (conteoFisico == null)
            {
                return NotFound();
            }

            return View(conteoFisico);
        }

        // GET: ConteoFisico/Create
        public IActionResult Create()
        {
            ViewData["AlmacenId"] = new SelectList(_context.Almacens, "Id", "Id");
            return View();
        }

        // POST: ConteoFisico/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AlmacenId,Fecha,Tipo,Estado,ResponsableId,CreadoEn")] ConteoFisico conteoFisico)
        {
            if (ModelState.IsValid)
            {
                conteoFisico.Id = Guid.NewGuid();
                _context.Add(conteoFisico);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AlmacenId"] = new SelectList(_context.Almacens, "Id", "Id", conteoFisico.AlmacenId);
            return View(conteoFisico);
        }

        // GET: ConteoFisico/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conteoFisico = await _context.ConteoFisicos.FindAsync(id);
            if (conteoFisico == null)
            {
                return NotFound();
            }
            ViewData["AlmacenId"] = new SelectList(_context.Almacens, "Id", "Id", conteoFisico.AlmacenId);
            return View(conteoFisico);
        }

        // POST: ConteoFisico/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,AlmacenId,Fecha,Tipo,Estado,ResponsableId,CreadoEn")] ConteoFisico conteoFisico)
        {
            if (id != conteoFisico.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(conteoFisico);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConteoFisicoExists(conteoFisico.Id))
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
            ViewData["AlmacenId"] = new SelectList(_context.Almacens, "Id", "Id", conteoFisico.AlmacenId);
            return View(conteoFisico);
        }

        // GET: ConteoFisico/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conteoFisico = await _context.ConteoFisicos
                .Include(c => c.Almacen)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (conteoFisico == null)
            {
                return NotFound();
            }

            return View(conteoFisico);
        }

        // POST: ConteoFisico/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var conteoFisico = await _context.ConteoFisicos.FindAsync(id);
            if (conteoFisico != null)
            {
                _context.ConteoFisicos.Remove(conteoFisico);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConteoFisicoExists(Guid id)
        {
            return _context.ConteoFisicos.Any(e => e.Id == id);
        }
    }
}
