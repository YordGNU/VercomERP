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
    public class PosVentaPendienteController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public PosVentaPendienteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PosVentaPendiente
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PosVentaPendientes.Include(p => p.DispositivoPos).Include(p => p.SesionCajaPos);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PosVentaPendiente/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posVentaPendiente = await _context.PosVentaPendientes
                .Include(p => p.DispositivoPos)
                .Include(p => p.SesionCajaPos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (posVentaPendiente == null)
            {
                return NotFound();
            }

            return View(posVentaPendiente);
        }

        // GET: PosVentaPendiente/Create
        public IActionResult Create()
        {
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id");
            ViewData["SesionCajaPosId"] = new SelectList(_context.SesionCajaPos, "Id", "Id");
            return View();
        }

        // POST: PosVentaPendiente/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DispositivoPosId,SesionCajaPosId,IdempotencyKey,PayloadJson,FechaVentaLocal,FechaRecibidoServidor,Estado,FacturaId,MensajeError,IntentosProcesamiento,ProcesadoEn")] PosVentaPendiente posVentaPendiente)
        {
            if (ModelState.IsValid)
            {
                posVentaPendiente.Id = Guid.NewGuid();
                _context.Add(posVentaPendiente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id", posVentaPendiente.DispositivoPosId);
            ViewData["SesionCajaPosId"] = new SelectList(_context.SesionCajaPos, "Id", "Id", posVentaPendiente.SesionCajaPosId);
            return View(posVentaPendiente);
        }

        // GET: PosVentaPendiente/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posVentaPendiente = await _context.PosVentaPendientes.FindAsync(id);
            if (posVentaPendiente == null)
            {
                return NotFound();
            }
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id", posVentaPendiente.DispositivoPosId);
            ViewData["SesionCajaPosId"] = new SelectList(_context.SesionCajaPos, "Id", "Id", posVentaPendiente.SesionCajaPosId);
            return View(posVentaPendiente);
        }

        // POST: PosVentaPendiente/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,DispositivoPosId,SesionCajaPosId,IdempotencyKey,PayloadJson,FechaVentaLocal,FechaRecibidoServidor,Estado,FacturaId,MensajeError,IntentosProcesamiento,ProcesadoEn")] PosVentaPendiente posVentaPendiente)
        {
            if (id != posVentaPendiente.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(posVentaPendiente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PosVentaPendienteExists(posVentaPendiente.Id))
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
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id", posVentaPendiente.DispositivoPosId);
            ViewData["SesionCajaPosId"] = new SelectList(_context.SesionCajaPos, "Id", "Id", posVentaPendiente.SesionCajaPosId);
            return View(posVentaPendiente);
        }

        // GET: PosVentaPendiente/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posVentaPendiente = await _context.PosVentaPendientes
                .Include(p => p.DispositivoPos)
                .Include(p => p.SesionCajaPos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (posVentaPendiente == null)
            {
                return NotFound();
            }

            return View(posVentaPendiente);
        }

        // POST: PosVentaPendiente/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var posVentaPendiente = await _context.PosVentaPendientes.FindAsync(id);
            if (posVentaPendiente != null)
            {
                _context.PosVentaPendientes.Remove(posVentaPendiente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PosVentaPendienteExists(Guid id)
        {
            return _context.PosVentaPendientes.Any(e => e.Id == id);
        }
    }
}
