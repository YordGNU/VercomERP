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
    public class SesionCajaPoController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public SesionCajaPoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SesionCajaPo
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.SesionCajaPos.Include(s => s.DispositivoPos);
            return View(await appDbContext.ToListAsync());
        }

        // GET: SesionCajaPo/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sesionCajaPo = await _context.SesionCajaPos
                .Include(s => s.DispositivoPos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sesionCajaPo == null)
            {
                return NotFound();
            }

            return View(sesionCajaPo);
        }

        // GET: SesionCajaPo/Create
        public IActionResult Create()
        {
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id");
            return View();
        }

        // POST: SesionCajaPo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DispositivoPosId,CajeroId,FechaApertura,MontoApertura,FechaCierre,MontoCierreDeclarado,MontoCierreSistema,DiferenciaArqueo,TotalVentas,TotalEfectivo,TotalTransfermovil,TotalEnzona,TotalOtrosMedios,CantidadFacturas,Estado,AsientoCierreId,ObservacionesCierre,SupervisorConciliacionId")] SesionCajaPo sesionCajaPo)
        {
            if (ModelState.IsValid)
            {
                sesionCajaPo.Id = Guid.NewGuid();
                _context.Add(sesionCajaPo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id", sesionCajaPo.DispositivoPosId);
            return View(sesionCajaPo);
        }

        // GET: SesionCajaPo/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sesionCajaPo = await _context.SesionCajaPos.FindAsync(id);
            if (sesionCajaPo == null)
            {
                return NotFound();
            }
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id", sesionCajaPo.DispositivoPosId);
            return View(sesionCajaPo);
        }

        // POST: SesionCajaPo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,DispositivoPosId,CajeroId,FechaApertura,MontoApertura,FechaCierre,MontoCierreDeclarado,MontoCierreSistema,DiferenciaArqueo,TotalVentas,TotalEfectivo,TotalTransfermovil,TotalEnzona,TotalOtrosMedios,CantidadFacturas,Estado,AsientoCierreId,ObservacionesCierre,SupervisorConciliacionId")] SesionCajaPo sesionCajaPo)
        {
            if (id != sesionCajaPo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sesionCajaPo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SesionCajaPoExists(sesionCajaPo.Id))
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
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id", sesionCajaPo.DispositivoPosId);
            return View(sesionCajaPo);
        }

        // GET: SesionCajaPo/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sesionCajaPo = await _context.SesionCajaPos
                .Include(s => s.DispositivoPos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sesionCajaPo == null)
            {
                return NotFound();
            }

            return View(sesionCajaPo);
        }

        // POST: SesionCajaPo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var sesionCajaPo = await _context.SesionCajaPos.FindAsync(id);
            if (sesionCajaPo != null)
            {
                _context.SesionCajaPos.Remove(sesionCajaPo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SesionCajaPoExists(Guid id)
        {
            return _context.SesionCajaPos.Any(e => e.Id == id);
        }
    }
}
