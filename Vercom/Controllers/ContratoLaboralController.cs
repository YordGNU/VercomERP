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
    public class ContratoLaboralController : Controller
    {
        private readonly AppDbContext _context;

        public ContratoLaboralController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ContratoLaboral
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ContratoLaborals.Include(c => c.Cargo).Include(c => c.Empleado);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ContratoLaboral/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contratoLaboral = await _context.ContratoLaborals
                .Include(c => c.Cargo)
                .Include(c => c.Empleado)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contratoLaboral == null)
            {
                return NotFound();
            }

            return View(contratoLaboral);
        }

        // GET: ContratoLaboral/Create
        public IActionResult Create()
        {
            ViewData["CargoId"] = new SelectList(_context.Cargos, "Id", "Id");
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id");
            return View();
        }

        // POST: ContratoLaboral/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpleadoId,TipoContrato,FechaInicio,FechaFin,SalarioPactado,JornadaHorasSemana,CargoId,DocumentoUrl,Estado,CreadoEn")] ContratoLaboral contratoLaboral)
        {
            if (ModelState.IsValid)
            {
                contratoLaboral.Id = Guid.NewGuid();
                _context.Add(contratoLaboral);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CargoId"] = new SelectList(_context.Cargos, "Id", "Id", contratoLaboral.CargoId);
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", contratoLaboral.EmpleadoId);
            return View(contratoLaboral);
        }

        // GET: ContratoLaboral/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contratoLaboral = await _context.ContratoLaborals.FindAsync(id);
            if (contratoLaboral == null)
            {
                return NotFound();
            }
            ViewData["CargoId"] = new SelectList(_context.Cargos, "Id", "Id", contratoLaboral.CargoId);
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", contratoLaboral.EmpleadoId);
            return View(contratoLaboral);
        }

        // POST: ContratoLaboral/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EmpleadoId,TipoContrato,FechaInicio,FechaFin,SalarioPactado,JornadaHorasSemana,CargoId,DocumentoUrl,Estado,CreadoEn")] ContratoLaboral contratoLaboral)
        {
            if (id != contratoLaboral.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contratoLaboral);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContratoLaboralExists(contratoLaboral.Id))
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
            ViewData["CargoId"] = new SelectList(_context.Cargos, "Id", "Id", contratoLaboral.CargoId);
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", contratoLaboral.EmpleadoId);
            return View(contratoLaboral);
        }

        // GET: ContratoLaboral/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contratoLaboral = await _context.ContratoLaborals
                .Include(c => c.Cargo)
                .Include(c => c.Empleado)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contratoLaboral == null)
            {
                return NotFound();
            }

            return View(contratoLaboral);
        }

        // POST: ContratoLaboral/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var contratoLaboral = await _context.ContratoLaborals.FindAsync(id);
            if (contratoLaboral != null)
            {
                _context.ContratoLaborals.Remove(contratoLaboral);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContratoLaboralExists(Guid id)
        {
            return _context.ContratoLaborals.Any(e => e.Id == id);
        }
    }
}
