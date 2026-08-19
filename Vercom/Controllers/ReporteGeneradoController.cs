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
    public class ReporteGeneradoController : Controller
    {
        private readonly AppDbContext _context;

        public ReporteGeneradoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ReporteGenerado
        public async Task<IActionResult> Index()
        {
            return View(await _context.ReporteGenerados.ToListAsync());
        }

        // GET: ReporteGenerado/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reporteGenerado = await _context.ReporteGenerados
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reporteGenerado == null)
            {
                return NotFound();
            }

            return View(reporteGenerado);
        }

        // GET: ReporteGenerado/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ReporteGenerado/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,NombreReporte,Formato,ParametrosJson,RutaArchivo,GeneradoPor,GeneradoEn")] ReporteGenerado reporteGenerado)
        {
            if (ModelState.IsValid)
            {
                reporteGenerado.Id = Guid.NewGuid();
                _context.Add(reporteGenerado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(reporteGenerado);
        }

        // GET: ReporteGenerado/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reporteGenerado = await _context.ReporteGenerados.FindAsync(id);
            if (reporteGenerado == null)
            {
                return NotFound();
            }
            return View(reporteGenerado);
        }

        // POST: ReporteGenerado/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,NombreReporte,Formato,ParametrosJson,RutaArchivo,GeneradoPor,GeneradoEn")] ReporteGenerado reporteGenerado)
        {
            if (id != reporteGenerado.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reporteGenerado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReporteGeneradoExists(reporteGenerado.Id))
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
            return View(reporteGenerado);
        }

        // GET: ReporteGenerado/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reporteGenerado = await _context.ReporteGenerados
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reporteGenerado == null)
            {
                return NotFound();
            }

            return View(reporteGenerado);
        }

        // POST: ReporteGenerado/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var reporteGenerado = await _context.ReporteGenerados.FindAsync(id);
            if (reporteGenerado != null)
            {
                _context.ReporteGenerados.Remove(reporteGenerado);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReporteGeneradoExists(Guid id)
        {
            return _context.ReporteGenerados.Any(e => e.Id == id);
        }
    }
}
