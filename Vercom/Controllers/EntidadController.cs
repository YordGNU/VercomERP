using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class EntidadController : Controller
    {
        private readonly AppDbContext _context;

        public EntidadController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Entidad
        public async Task<IActionResult> Index()
        {
            return View(await _context.Entidads.ToListAsync());
        }

        // GET: Entidad/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entidad = await _context.Entidads
                .FirstOrDefaultAsync(m => m.Id == id);
            if (entidad == null)
            {
                return NotFound();
            }

            return View(entidad);
        }

        // GET: Entidad/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Entidad/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RazonSocial,NombreComercial,Nit,CodigoReeup,FormaJuridica,DireccionLegal,Municipio,Provincia,Telefono,Email,FechaConstitucion,LicenciaActividad,MonedaBase,Activo,CreadoEn,ActualizadoEn")] Entidad entidad)
        {
            if (ModelState.IsValid)
            {
                entidad.Id = Guid.NewGuid();
                _context.Add(entidad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(entidad);
        }

        // GET: Entidad/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entidad = await _context.Entidads.FindAsync(id);
            if (entidad == null)
            {
                return NotFound();
            }
            return View(entidad);
        }

        // POST: Entidad/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,RazonSocial,NombreComercial,Nit,CodigoReeup,FormaJuridica,DireccionLegal,Municipio,Provincia,Telefono,Email,FechaConstitucion,LicenciaActividad,MonedaBase,Activo,CreadoEn,ActualizadoEn")] Entidad entidad)
        {
            if (id != entidad.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entidad);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntidadExists(entidad.Id))
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
            return View(entidad);
        }

        // GET: Entidad/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entidad = await _context.Entidads
                .FirstOrDefaultAsync(m => m.Id == id);
            if (entidad == null)
            {
                return NotFound();
            }

            return View(entidad);
        }

        // POST: Entidad/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var entidad = await _context.Entidads.FindAsync(id);
            if (entidad != null)
            {
                _context.Entidads.Remove(entidad);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EntidadExists(Guid id)
        {
            return _context.Entidads.Any(e => e.Id == id);
        }
    }
}
