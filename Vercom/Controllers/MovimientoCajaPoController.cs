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
    public class MovimientoCajaPoController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public MovimientoCajaPoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: MovimientoCajaPo
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.MovimientoCajaPos.Include(m => m.SesionCajaPos);
            return View(await appDbContext.ToListAsync());
        }

        // GET: MovimientoCajaPo/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimientoCajaPo = await _context.MovimientoCajaPos
                .Include(m => m.SesionCajaPos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movimientoCajaPo == null)
            {
                return NotFound();
            }

            return View(movimientoCajaPo);
        }

        // GET: MovimientoCajaPo/Create
        public IActionResult Create()
        {
            ViewData["SesionCajaPosId"] = new SelectList(_context.SesionCajaPos, "Id", "Id");
            return View();
        }

        // POST: MovimientoCajaPo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SesionCajaPosId,Tipo,Monto,FacturaId,Motivo,AutorizadoPor,OcurridoEn")] MovimientoCajaPo movimientoCajaPo)
        {
            if (ModelState.IsValid)
            {
                movimientoCajaPo.Id = Guid.NewGuid();
                _context.Add(movimientoCajaPo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SesionCajaPosId"] = new SelectList(_context.SesionCajaPos, "Id", "Id", movimientoCajaPo.SesionCajaPosId);
            return View(movimientoCajaPo);
        }

        // GET: MovimientoCajaPo/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimientoCajaPo = await _context.MovimientoCajaPos.FindAsync(id);
            if (movimientoCajaPo == null)
            {
                return NotFound();
            }
            ViewData["SesionCajaPosId"] = new SelectList(_context.SesionCajaPos, "Id", "Id", movimientoCajaPo.SesionCajaPosId);
            return View(movimientoCajaPo);
        }

        // POST: MovimientoCajaPo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,SesionCajaPosId,Tipo,Monto,FacturaId,Motivo,AutorizadoPor,OcurridoEn")] MovimientoCajaPo movimientoCajaPo)
        {
            if (id != movimientoCajaPo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movimientoCajaPo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovimientoCajaPoExists(movimientoCajaPo.Id))
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
            ViewData["SesionCajaPosId"] = new SelectList(_context.SesionCajaPos, "Id", "Id", movimientoCajaPo.SesionCajaPosId);
            return View(movimientoCajaPo);
        }

        // GET: MovimientoCajaPo/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimientoCajaPo = await _context.MovimientoCajaPos
                .Include(m => m.SesionCajaPos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movimientoCajaPo == null)
            {
                return NotFound();
            }

            return View(movimientoCajaPo);
        }

        // POST: MovimientoCajaPo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var movimientoCajaPo = await _context.MovimientoCajaPos.FindAsync(id);
            if (movimientoCajaPo != null)
            {
                _context.MovimientoCajaPos.Remove(movimientoCajaPo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovimientoCajaPoExists(Guid id)
        {
            return _context.MovimientoCajaPos.Any(e => e.Id == id);
        }
    }
}
