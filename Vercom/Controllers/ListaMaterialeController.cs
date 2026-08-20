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
    public class ListaMaterialeController : Controller
    {
        private readonly AppDbContext _context;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public ListaMaterialeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ListaMateriale
        [Authorize(Policy = "PRODUCCION.BOM.VER")]
        public async Task<IActionResult> Index()
        {
            var boms = await _context.ListaMateriales
                .Include(l => l.ProductoTerminado)
                .Where(l => l.ProductoTerminado.EntidadId == CurrentEntidadId)
                .OrderBy(l => l.ProductoTerminado.Nombre)
                .ToListAsync();
            return View(boms);
        }

        // GET: ListaMateriale/Details/5
        [Authorize(Policy = "PRODUCCION.BOM.VER")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var listaMateriale = await _context.ListaMateriales
                .Include(l => l.ProductoTerminado)
                .Include(l => l.ListaMaterialesDetalles).ThenInclude(d => d.ProductoInsumo).ThenInclude(p => p.UnidadMedida)
                .FirstOrDefaultAsync(m => m.Id == id && m.ProductoTerminado.EntidadId == CurrentEntidadId);

            if (listaMateriale == null) return NotFound();

            return View(listaMateriale);
        }

        // GET: ListaMateriale/Create
        [Authorize(Policy = "PRODUCCION.BOM.CREAR")]
        public IActionResult Create()
        {
            ViewData["ProductoTerminadoId"] = new SelectList(_context.Productos
                .Where(p => p.EntidadId == CurrentEntidadId && (p.Tipo == "ELABORADO" || p.Tipo == "TERMINADO")), "Id", "Nombre");

            ViewBag.Insumos = _context.Productos
                .Where(p => p.EntidadId == CurrentEntidadId && p.Tipo == "INSUMO")
                .Select(p => new { p.Id, Display = p.Codigo + " - " + p.Nombre })
                .ToList();

            return View(new ListaMateriale { Activa = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "PRODUCCION.BOM.CREAR")]
        public async Task<IActionResult> Create(ListaMateriale bom)
        {
            if (ModelState.IsValid)
            {
                bom.Id = Guid.NewGuid();
                bom.CreadoEn = DateTimeOffset.Now;

                // Desactivar versiones anteriores
                var previous = await _context.ListaMateriales
                    .Where(l => l.ProductoTerminadoId == bom.ProductoTerminadoId && l.Activa)
                    .ToListAsync();
                foreach (var p in previous) p.Activa = false;

                _context.Add(bom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bom);
        }

        private bool ListaMaterialeExists(Guid id)
        {
            return _context.ListaMateriales.Any(e => e.Id == id);
        }
    }
}
