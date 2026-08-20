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
    public class RegistroSalarioTiempoServicioController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public RegistroSalarioTiempoServicioController(AppDbContext context)
        {
            _context = context;
        }

        // GET: RegistroSalarioTiempoServicio
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.RegistroSalarioTiempoServicios.Include(r => r.Empleado);
            return View(await appDbContext.ToListAsync());
        }

        // GET: RegistroSalarioTiempoServicio/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registroSalarioTiempoServicio = await _context.RegistroSalarioTiempoServicios
                .Include(r => r.Empleado)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (registroSalarioTiempoServicio == null)
            {
                return NotFound();
            }

            return View(registroSalarioTiempoServicio);
        }

        // GET: RegistroSalarioTiempoServicio/Create
        public IActionResult Create()
        {
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id");
            return View();
        }

        // POST: RegistroSalarioTiempoServicio/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpleadoId,Anio,Mes,DiasTrabajados,SalarioDevengado,TiempoServicioAcumuladoMeses")] RegistroSalarioTiempoServicio registroSalarioTiempoServicio)
        {
            if (ModelState.IsValid)
            {
                registroSalarioTiempoServicio.Id = Guid.NewGuid();
                _context.Add(registroSalarioTiempoServicio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", registroSalarioTiempoServicio.EmpleadoId);
            return View(registroSalarioTiempoServicio);
        }

        // GET: RegistroSalarioTiempoServicio/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registroSalarioTiempoServicio = await _context.RegistroSalarioTiempoServicios.FindAsync(id);
            if (registroSalarioTiempoServicio == null)
            {
                return NotFound();
            }
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", registroSalarioTiempoServicio.EmpleadoId);
            return View(registroSalarioTiempoServicio);
        }

        // POST: RegistroSalarioTiempoServicio/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EmpleadoId,Anio,Mes,DiasTrabajados,SalarioDevengado,TiempoServicioAcumuladoMeses")] RegistroSalarioTiempoServicio registroSalarioTiempoServicio)
        {
            if (id != registroSalarioTiempoServicio.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(registroSalarioTiempoServicio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegistroSalarioTiempoServicioExists(registroSalarioTiempoServicio.Id))
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
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", registroSalarioTiempoServicio.EmpleadoId);
            return View(registroSalarioTiempoServicio);
        }

        // GET: RegistroSalarioTiempoServicio/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registroSalarioTiempoServicio = await _context.RegistroSalarioTiempoServicios
                .Include(r => r.Empleado)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (registroSalarioTiempoServicio == null)
            {
                return NotFound();
            }

            return View(registroSalarioTiempoServicio);
        }

        // POST: RegistroSalarioTiempoServicio/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var registroSalarioTiempoServicio = await _context.RegistroSalarioTiempoServicios.FindAsync(id);
            if (registroSalarioTiempoServicio != null)
            {
                _context.RegistroSalarioTiempoServicios.Remove(registroSalarioTiempoServicio);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RegistroSalarioTiempoServicioExists(Guid id)
        {
            return _context.RegistroSalarioTiempoServicios.Any(e => e.Id == id);
        }
    }
}
