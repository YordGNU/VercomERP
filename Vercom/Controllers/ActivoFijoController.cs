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
    public class ActivoFijoController : Controller
    {
      private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());  

        public ActivoFijoController(AppDbContext context)
        {
            _context = context;
        }



        // GET: ActivoFijo
        public async Task<IActionResult> Index()
        {         
            var appDbContext = _context.ActivoFijos.Include(a => a.CuentaActivo).Include(a => a.CuentaDepreciacion).Include(a => a.CuentaGastoDep);          
            return View(await appDbContext.ToListAsync());
        }

        // GET: ActivoFijo/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activoFijo = await _context.ActivoFijos
                .Include(a => a.CuentaActivo)
                .Include(a => a.CuentaDepreciacion)
                .Include(a => a.CuentaGastoDep)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (activoFijo == null)
            {
                return NotFound();
            }

            return View(activoFijo);
        }

        // GET: ActivoFijo/Create
        public IActionResult Create()
        {
            ViewData["CuentaActivoId"] = new SelectList(_context.CuentaContables, "Id", "Id");
            ViewData["CuentaDepreciacionId"] = new SelectList(_context.CuentaContables, "Id", "Id");
            ViewData["CuentaGastoDepId"] = new SelectList(_context.CuentaContables, "Id", "Id");
            ViewData["SucursalId"] = new SelectList(_context.Sucursals.Where(s => s.EntidadId == CurrentEntidadId), "Id", "Nombre");
            return View();
        }

        // POST: ActivoFijo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,SucursalId,CodigoInventario,Descripcion,CuentaActivoId,CuentaDepreciacionId,CuentaGastoDepId,FechaAdquisicion,ValorAdquisicion,ValorResidual,VidaUtilMeses,TasaDepreciacionAnual,MetodoDepreciacion,DepreciacionAcumulada,Estado,FechaBaja,MotivoBaja")] ActivoFijo activoFijo)
        {
            if (ModelState.IsValid)
            {
                activoFijo.Id = Guid.NewGuid();
                _context.Add(activoFijo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CuentaActivoId"] = new SelectList(_context.CuentaContables, "Id", "Id", activoFijo.CuentaActivoId);
            ViewData["CuentaDepreciacionId"] = new SelectList(_context.CuentaContables, "Id", "Id", activoFijo.CuentaDepreciacionId);
            ViewData["CuentaGastoDepId"] = new SelectList(_context.CuentaContables, "Id", "Id", activoFijo.CuentaGastoDepId);
            return View(activoFijo);
        }

        // GET: ActivoFijo/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activoFijo = await _context.ActivoFijos.FindAsync(id);
            if (activoFijo == null)
            {
                return NotFound();
            }
            ViewData["CuentaActivoId"] = new SelectList(_context.CuentaContables, "Id", "Id", activoFijo.CuentaActivoId);
            ViewData["CuentaDepreciacionId"] = new SelectList(_context.CuentaContables, "Id", "Id", activoFijo.CuentaDepreciacionId);
            ViewData["CuentaGastoDepId"] = new SelectList(_context.CuentaContables, "Id", "Id", activoFijo.CuentaGastoDepId);
            return View(activoFijo);
        }

        // POST: ActivoFijo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,SucursalId,CodigoInventario,Descripcion,CuentaActivoId,CuentaDepreciacionId,CuentaGastoDepId,FechaAdquisicion,ValorAdquisicion,ValorResidual,VidaUtilMeses,TasaDepreciacionAnual,MetodoDepreciacion,DepreciacionAcumulada,Estado,FechaBaja,MotivoBaja")] ActivoFijo activoFijo)
        {
            if (id != activoFijo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(activoFijo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActivoFijoExists(activoFijo.Id))
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
            ViewData["CuentaActivoId"] = new SelectList(_context.CuentaContables, "Id", "Id", activoFijo.CuentaActivoId);
            ViewData["CuentaDepreciacionId"] = new SelectList(_context.CuentaContables, "Id", "Id", activoFijo.CuentaDepreciacionId);
            ViewData["CuentaGastoDepId"] = new SelectList(_context.CuentaContables, "Id", "Id", activoFijo.CuentaGastoDepId);
            return View(activoFijo);
        }

        // GET: ActivoFijo/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activoFijo = await _context.ActivoFijos
                .Include(a => a.CuentaActivo)
                .Include(a => a.CuentaDepreciacion)
                .Include(a => a.CuentaGastoDep)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (activoFijo == null)
            {
                return NotFound();
            }

            return View(activoFijo);
        }

        // POST: ActivoFijo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var activoFijo = await _context.ActivoFijos.FindAsync(id);
            if (activoFijo != null)
            {
                _context.ActivoFijos.Remove(activoFijo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActivoFijoExists(Guid id)
        {
            return _context.ActivoFijos.Any(e => e.Id == id);
        }
    }
}
