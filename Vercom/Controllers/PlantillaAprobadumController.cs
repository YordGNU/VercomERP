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
    public class PlantillaAprobadumController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public PlantillaAprobadumController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PlantillaAprobadum
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PlantillaAprobada.Include(p => p.Cargo);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PlantillaAprobadum/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plantillaAprobadum = await _context.PlantillaAprobada
                .Include(p => p.Cargo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (plantillaAprobadum == null)
            {
                return NotFound();
            }

            return View(plantillaAprobadum);
        }

        // GET: PlantillaAprobadum/Create
        public IActionResult Create()
        {
            ViewData["CargoId"] = new SelectList(_context.Cargos, "Id", "Id");
            return View();
        }

        // POST: PlantillaAprobadum/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,SucursalId,CargoId,PlazasAprobadas,VigenteDesde,VigenteHasta")] PlantillaAprobadum plantillaAprobadum)
        {
            if (ModelState.IsValid)
            {
                plantillaAprobadum.Id = Guid.NewGuid();
                _context.Add(plantillaAprobadum);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CargoId"] = new SelectList(_context.Cargos, "Id", "Id", plantillaAprobadum.CargoId);
            return View(plantillaAprobadum);
        }

        // GET: PlantillaAprobadum/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plantillaAprobadum = await _context.PlantillaAprobada.FindAsync(id);
            if (plantillaAprobadum == null)
            {
                return NotFound();
            }
            ViewData["CargoId"] = new SelectList(_context.Cargos, "Id", "Id", plantillaAprobadum.CargoId);
            return View(plantillaAprobadum);
        }

        // POST: PlantillaAprobadum/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,SucursalId,CargoId,PlazasAprobadas,VigenteDesde,VigenteHasta")] PlantillaAprobadum plantillaAprobadum)
        {
            if (id != plantillaAprobadum.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plantillaAprobadum);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlantillaAprobadumExists(plantillaAprobadum.Id))
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
            ViewData["CargoId"] = new SelectList(_context.Cargos, "Id", "Id", plantillaAprobadum.CargoId);
            return View(plantillaAprobadum);
        }

        // GET: PlantillaAprobadum/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plantillaAprobadum = await _context.PlantillaAprobada
                .Include(p => p.Cargo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (plantillaAprobadum == null)
            {
                return NotFound();
            }

            return View(plantillaAprobadum);
        }

        // POST: PlantillaAprobadum/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var plantillaAprobadum = await _context.PlantillaAprobada.FindAsync(id);
            if (plantillaAprobadum != null)
            {
                _context.PlantillaAprobada.Remove(plantillaAprobadum);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlantillaAprobadumExists(Guid id)
        {
            return _context.PlantillaAprobada.Any(e => e.Id == id);
        }
    }
}
