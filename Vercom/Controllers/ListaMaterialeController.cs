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
    public class ListaMaterialeController : Controller
    {
        private readonly AppDbContext _context;

        public ListaMaterialeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ListaMateriale
        public async Task<IActionResult> Index()
        {
            return View(await _context.ListaMateriales.ToListAsync());
        }

        // GET: ListaMateriale/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listaMateriale = await _context.ListaMateriales
                .FirstOrDefaultAsync(m => m.Id == id);
            if (listaMateriale == null)
            {
                return NotFound();
            }

            return View(listaMateriale);
        }

        // GET: ListaMateriale/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ListaMateriale/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductoTerminadoId,Version,Activa,CreadoEn")] ListaMateriale listaMateriale)
        {
            if (ModelState.IsValid)
            {
                listaMateriale.Id = Guid.NewGuid();
                _context.Add(listaMateriale);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(listaMateriale);
        }

        // GET: ListaMateriale/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listaMateriale = await _context.ListaMateriales.FindAsync(id);
            if (listaMateriale == null)
            {
                return NotFound();
            }
            return View(listaMateriale);
        }

        // POST: ListaMateriale/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ProductoTerminadoId,Version,Activa,CreadoEn")] ListaMateriale listaMateriale)
        {
            if (id != listaMateriale.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listaMateriale);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListaMaterialeExists(listaMateriale.Id))
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
            return View(listaMateriale);
        }

        // GET: ListaMateriale/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listaMateriale = await _context.ListaMateriales
                .FirstOrDefaultAsync(m => m.Id == id);
            if (listaMateriale == null)
            {
                return NotFound();
            }

            return View(listaMateriale);
        }

        // POST: ListaMateriale/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var listaMateriale = await _context.ListaMateriales.FindAsync(id);
            if (listaMateriale != null)
            {
                _context.ListaMateriales.Remove(listaMateriale);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ListaMaterialeExists(Guid id)
        {
            return _context.ListaMateriales.Any(e => e.Id == id);
        }
    }
}
