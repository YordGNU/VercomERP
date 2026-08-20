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
    public class DevolucionVentumController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public DevolucionVentumController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DevolucionVentum
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.DevolucionVenta.Include(d => d.Factura);
            return View(await appDbContext.ToListAsync());
        }

        // GET: DevolucionVentum/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var devolucionVentum = await _context.DevolucionVenta
                .Include(d => d.Factura)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (devolucionVentum == null)
            {
                return NotFound();
            }

            return View(devolucionVentum);
        }

        // GET: DevolucionVentum/Create
        public IActionResult Create()
        {
            ViewData["FacturaId"] = new SelectList(_context.FacturaVenta, "Id", "Id");
            return View();
        }

        // POST: DevolucionVentum/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FacturaId,Fecha,Motivo,TotalDevuelto,MovimientoInventarioId,AsientoId,AutorizadoPor")] DevolucionVentum devolucionVentum)
        {
            if (ModelState.IsValid)
            {
                devolucionVentum.Id = Guid.NewGuid();
                _context.Add(devolucionVentum);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FacturaId"] = new SelectList(_context.FacturaVenta, "Id", "Id", devolucionVentum.FacturaId);
            return View(devolucionVentum);
        }

        // GET: DevolucionVentum/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var devolucionVentum = await _context.DevolucionVenta.FindAsync(id);
            if (devolucionVentum == null)
            {
                return NotFound();
            }
            ViewData["FacturaId"] = new SelectList(_context.FacturaVenta, "Id", "Id", devolucionVentum.FacturaId);
            return View(devolucionVentum);
        }

        // POST: DevolucionVentum/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,FacturaId,Fecha,Motivo,TotalDevuelto,MovimientoInventarioId,AsientoId,AutorizadoPor")] DevolucionVentum devolucionVentum)
        {
            if (id != devolucionVentum.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(devolucionVentum);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DevolucionVentumExists(devolucionVentum.Id))
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
            ViewData["FacturaId"] = new SelectList(_context.FacturaVenta, "Id", "Id", devolucionVentum.FacturaId);
            return View(devolucionVentum);
        }

        // GET: DevolucionVentum/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var devolucionVentum = await _context.DevolucionVenta
                .Include(d => d.Factura)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (devolucionVentum == null)
            {
                return NotFound();
            }

            return View(devolucionVentum);
        }

        // POST: DevolucionVentum/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var devolucionVentum = await _context.DevolucionVenta.FindAsync(id);
            if (devolucionVentum != null)
            {
                _context.DevolucionVenta.Remove(devolucionVentum);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DevolucionVentumExists(Guid id)
        {
            return _context.DevolucionVenta.Any(e => e.Id == id);
        }
    }
}
