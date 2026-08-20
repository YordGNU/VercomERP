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
    public class FormaPagoVentumController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public FormaPagoVentumController(AppDbContext context)
        {
            _context = context;
        }

        // GET: FormaPagoVentum
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.FormaPagoVenta.Include(f => f.Factura);
            return View(await appDbContext.ToListAsync());
        }

        // GET: FormaPagoVentum/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var formaPagoVentum = await _context.FormaPagoVenta
                .Include(f => f.Factura)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (formaPagoVentum == null)
            {
                return NotFound();
            }

            return View(formaPagoVentum);
        }

        // GET: FormaPagoVentum/Create
        public IActionResult Create()
        {
            ViewData["FacturaId"] = new SelectList(_context.FacturaVenta, "Id", "Id");
            return View();
        }

        // POST: FormaPagoVentum/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FacturaId,FormaPago,Monto,ReferenciaExterna,VueltoEntregado")] FormaPagoVentum formaPagoVentum)
        {
            if (ModelState.IsValid)
            {
                formaPagoVentum.Id = Guid.NewGuid();
                _context.Add(formaPagoVentum);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FacturaId"] = new SelectList(_context.FacturaVenta, "Id", "Id", formaPagoVentum.FacturaId);
            return View(formaPagoVentum);
        }

        // GET: FormaPagoVentum/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var formaPagoVentum = await _context.FormaPagoVenta.FindAsync(id);
            if (formaPagoVentum == null)
            {
                return NotFound();
            }
            ViewData["FacturaId"] = new SelectList(_context.FacturaVenta, "Id", "Id", formaPagoVentum.FacturaId);
            return View(formaPagoVentum);
        }

        // POST: FormaPagoVentum/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,FacturaId,FormaPago,Monto,ReferenciaExterna,VueltoEntregado")] FormaPagoVentum formaPagoVentum)
        {
            if (id != formaPagoVentum.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(formaPagoVentum);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FormaPagoVentumExists(formaPagoVentum.Id))
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
            ViewData["FacturaId"] = new SelectList(_context.FacturaVenta, "Id", "Id", formaPagoVentum.FacturaId);
            return View(formaPagoVentum);
        }

        // GET: FormaPagoVentum/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var formaPagoVentum = await _context.FormaPagoVenta
                .Include(f => f.Factura)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (formaPagoVentum == null)
            {
                return NotFound();
            }

            return View(formaPagoVentum);
        }

        // POST: FormaPagoVentum/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var formaPagoVentum = await _context.FormaPagoVenta.FindAsync(id);
            if (formaPagoVentum != null)
            {
                _context.FormaPagoVenta.Remove(formaPagoVentum);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FormaPagoVentumExists(Guid id)
        {
            return _context.FormaPagoVenta.Any(e => e.Id == id);
        }
    }
}
