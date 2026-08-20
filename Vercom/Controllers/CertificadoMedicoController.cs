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
    public class CertificadoMedicoController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public CertificadoMedicoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CertificadoMedico
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.CertificadoMedicos.Include(c => c.Empleado);
            return View(await appDbContext.ToListAsync());
        }

        // GET: CertificadoMedico/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificadoMedico = await _context.CertificadoMedicos
                .Include(c => c.Empleado)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificadoMedico == null)
            {
                return NotFound();
            }

            return View(certificadoMedico);
        }

        // GET: CertificadoMedico/Create
        public IActionResult Create()
        {
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id");
            return View();
        }

        // POST: CertificadoMedico/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpleadoId,FechaInicio,FechaFin,Dias,DiagnosticoCie,PorcentajeSubsidio,NumeroCertificado,CreadoEn")] CertificadoMedico certificadoMedico)
        {
            if (ModelState.IsValid)
            {
                certificadoMedico.Id = Guid.NewGuid();
                _context.Add(certificadoMedico);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", certificadoMedico.EmpleadoId);
            return View(certificadoMedico);
        }

        // GET: CertificadoMedico/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificadoMedico = await _context.CertificadoMedicos.FindAsync(id);
            if (certificadoMedico == null)
            {
                return NotFound();
            }
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", certificadoMedico.EmpleadoId);
            return View(certificadoMedico);
        }

        // POST: CertificadoMedico/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EmpleadoId,FechaInicio,FechaFin,Dias,DiagnosticoCie,PorcentajeSubsidio,NumeroCertificado,CreadoEn")] CertificadoMedico certificadoMedico)
        {
            if (id != certificadoMedico.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(certificadoMedico);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CertificadoMedicoExists(certificadoMedico.Id))
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
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", certificadoMedico.EmpleadoId);
            return View(certificadoMedico);
        }

        // GET: CertificadoMedico/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificadoMedico = await _context.CertificadoMedicos
                .Include(c => c.Empleado)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificadoMedico == null)
            {
                return NotFound();
            }

            return View(certificadoMedico);
        }

        // POST: CertificadoMedico/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var certificadoMedico = await _context.CertificadoMedicos.FindAsync(id);
            if (certificadoMedico != null)
            {
                _context.CertificadoMedicos.Remove(certificadoMedico);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CertificadoMedicoExists(Guid id)
        {
            return _context.CertificadoMedicos.Any(e => e.Id == id);
        }
    }
}
