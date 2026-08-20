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
    [Authorize]
    public class MermaController : Controller
    {
        private readonly AppDbContext _context;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public MermaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Merma
        [Authorize(Policy = "PRODUCCION.ORDEN.VER")]
        public async Task<IActionResult> Index()
        {
            var mermas = await _context.Mermas
                .Include(m => m.OrdenProduccion)
                .Include(m => m.Producto)
                .Where(m => m.OrdenProduccion.EntidadId == CurrentEntidadId)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
            return View(mermas);
        }

        // GET: Merma/Create
        [Authorize(Policy = "PRODUCCION.MERMA.REGISTRAR")]
        public IActionResult Create(Guid? opId)
        {
            var ops = _context.OrdenProduccions
                .Where(o => o.EntidadId == CurrentEntidadId && o.Estado == "EN_PROCESO")
                .Select(o => new { o.Id, Display = o.NumeroOrden + " - " + o.ProductoTerminado.Nombre })
                .ToList();

            ViewData["OrdenProduccionId"] = new SelectList(ops, "Id", "Display", opId);

            ViewData["ProductoId"] = new SelectList(_context.Productos
                .Where(p => p.EntidadId == CurrentEntidadId && p.Activo), "Id", "Nombre");

            return View(new Merma { Fecha = DateOnly.FromDateTime(DateTime.Now) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "PRODUCCION.MERMA.REGISTRAR")]
        public async Task<IActionResult> Create(Merma merma)
        {
            if (ModelState.IsValid)
            {
                merma.Id = Guid.NewGuid();
                _context.Add(merma);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Merma registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(merma);
        }

        private bool MermaExists(Guid id)
        {
            return _context.Mermas.Any(e => e.Id == id);
        }
    }
}
