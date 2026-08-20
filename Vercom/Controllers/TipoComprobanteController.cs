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
    public class TipoComprobanteController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public TipoComprobanteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: TipoComprobante
        public async Task<IActionResult> Index()
        {
            return View(await _context.TipoComprobantes.ToListAsync());
        }

        // GET: TipoComprobante/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoComprobante = await _context.TipoComprobantes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoComprobante == null)
            {
                return NotFound();
            }

            return View(tipoComprobante);
        }

        // GET: TipoComprobante/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TipoComprobante/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Codigo,Nombre")] TipoComprobante tipoComprobante)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tipoComprobante);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tipoComprobante);
        }

        // GET: TipoComprobante/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoComprobante = await _context.TipoComprobantes.FindAsync(id);
            if (tipoComprobante == null)
            {
                return NotFound();
            }
            return View(tipoComprobante);
        }

        // POST: TipoComprobante/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nombre")] TipoComprobante tipoComprobante)
        {
            if (id != tipoComprobante.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoComprobante);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoComprobanteExists(tipoComprobante.Id))
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
            return View(tipoComprobante);
        }

        // GET: TipoComprobante/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoComprobante = await _context.TipoComprobantes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoComprobante == null)
            {
                return NotFound();
            }

            return View(tipoComprobante);
        }

        // POST: TipoComprobante/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tipoComprobante = await _context.TipoComprobantes.FindAsync(id);
            if (tipoComprobante != null)
            {
                _context.TipoComprobantes.Remove(tipoComprobante);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipoComprobanteExists(int id)
        {
            return _context.TipoComprobantes.Any(e => e.Id == id);
        }
    }
}
