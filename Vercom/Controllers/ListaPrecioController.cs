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
    public class ListaPrecioController : Controller
    {
        private readonly AppDbContext _context;

        public ListaPrecioController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ListaPrecio
        public async Task<IActionResult> Index()
        {
            return View(await _context.ListaPrecios.ToListAsync());
        }

        // GET: ListaPrecio/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listaPrecio = await _context.ListaPrecios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (listaPrecio == null)
            {
                return NotFound();
            }

            return View(listaPrecio);
        }

        // GET: ListaPrecio/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ListaPrecio/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,Nombre,Canal,VigenteDesde,VigenteHasta,Activa")] ListaPrecio listaPrecio)
        {
            if (ModelState.IsValid)
            {
                listaPrecio.Id = Guid.NewGuid();
                _context.Add(listaPrecio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(listaPrecio);
        }

        // GET: ListaPrecio/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listaPrecio = await _context.ListaPrecios.FindAsync(id);
            if (listaPrecio == null)
            {
                return NotFound();
            }
            return View(listaPrecio);
        }

        // POST: ListaPrecio/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,Nombre,Canal,VigenteDesde,VigenteHasta,Activa")] ListaPrecio listaPrecio)
        {
            if (id != listaPrecio.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listaPrecio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListaPrecioExists(listaPrecio.Id))
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
            return View(listaPrecio);
        }

        // GET: ListaPrecio/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listaPrecio = await _context.ListaPrecios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (listaPrecio == null)
            {
                return NotFound();
            }

            return View(listaPrecio);
        }

        // POST: ListaPrecio/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var listaPrecio = await _context.ListaPrecios.FindAsync(id);
            if (listaPrecio != null)
            {
                _context.ListaPrecios.Remove(listaPrecio);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ListaPrecioExists(Guid id)
        {
            return _context.ListaPrecios.Any(e => e.Id == id);
        }
    }
}
