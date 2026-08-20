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
    public class RegistroAsistenciumController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public RegistroAsistenciumController(AppDbContext context)
        {
            _context = context;
        }

        // GET: RegistroAsistencium
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.RegistroAsistencia.Include(r => r.Empleado).Include(r => r.TipoAusencia);
            return View(await appDbContext.ToListAsync());
        }

        // GET: RegistroAsistencium/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registroAsistencium = await _context.RegistroAsistencia
                .Include(r => r.Empleado)
                .Include(r => r.TipoAusencia)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (registroAsistencium == null)
            {
                return NotFound();
            }

            return View(registroAsistencium);
        }

        // GET: RegistroAsistencium/Create
        public IActionResult Create()
        {
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id");
            ViewData["TipoAusenciaId"] = new SelectList(_context.TipoAusencia, "Id", "Id");
            return View();
        }

        // POST: RegistroAsistencium/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpleadoId,Fecha,HoraEntrada,HoraSalida,HorasExtra,TipoAusenciaId,Observaciones,RegistradoPor")] RegistroAsistencium registroAsistencium)
        {
            if (ModelState.IsValid)
            {
                registroAsistencium.Id = Guid.NewGuid();
                _context.Add(registroAsistencium);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", registroAsistencium.EmpleadoId);
            ViewData["TipoAusenciaId"] = new SelectList(_context.TipoAusencia, "Id", "Id", registroAsistencium.TipoAusenciaId);
            return View(registroAsistencium);
        }

        // GET: RegistroAsistencium/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registroAsistencium = await _context.RegistroAsistencia.FindAsync(id);
            if (registroAsistencium == null)
            {
                return NotFound();
            }
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", registroAsistencium.EmpleadoId);
            ViewData["TipoAusenciaId"] = new SelectList(_context.TipoAusencia, "Id", "Id", registroAsistencium.TipoAusenciaId);
            return View(registroAsistencium);
        }

        // POST: RegistroAsistencium/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EmpleadoId,Fecha,HoraEntrada,HoraSalida,HorasExtra,TipoAusenciaId,Observaciones,RegistradoPor")] RegistroAsistencium registroAsistencium)
        {
            if (id != registroAsistencium.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(registroAsistencium);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegistroAsistenciumExists(registroAsistencium.Id))
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
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", registroAsistencium.EmpleadoId);
            ViewData["TipoAusenciaId"] = new SelectList(_context.TipoAusencia, "Id", "Id", registroAsistencium.TipoAusenciaId);
            return View(registroAsistencium);
        }

        // GET: RegistroAsistencium/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registroAsistencium = await _context.RegistroAsistencia
                .Include(r => r.Empleado)
                .Include(r => r.TipoAusencia)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (registroAsistencium == null)
            {
                return NotFound();
            }

            return View(registroAsistencium);
        }

        // POST: RegistroAsistencium/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var registroAsistencium = await _context.RegistroAsistencia.FindAsync(id);
            if (registroAsistencium != null)
            {
                _context.RegistroAsistencia.Remove(registroAsistencium);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RegistroAsistenciumExists(Guid id)
        {
            return _context.RegistroAsistencia.Any(e => e.Id == id);
        }
    }
}
