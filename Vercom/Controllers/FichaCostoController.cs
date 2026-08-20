using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers
{
    [Authorize]
    public class FichaCostoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProductionService _productionService;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public FichaCostoController(AppDbContext context, IProductionService productionService)
        {
            _context = context;
            _productionService = productionService;
        }

        // GET: FichaCosto
        [Authorize(Policy = "PRODUCCION.FICHA.VER")]
        public async Task<IActionResult> Index()
        {
            var fichas = await _context.FichaCostos
                .Include(f => f.Producto)
                .Where(f => f.EntidadId == CurrentEntidadId)
                .OrderBy(f => f.Producto.Nombre).ThenByDescending(f => f.Version)
                .ToListAsync();
            return View(fichas);
        }

        // GET: FichaCosto/Details/5
        [Authorize(Policy = "PRODUCCION.FICHA.VER")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var fichaCosto = await _context.FichaCostos
                .Include(f => f.Producto).ThenInclude(p => p.UnidadMedida)
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (fichaCosto == null) return NotFound();

            return View(fichaCosto);
        }

        // GET: FichaCosto/Create
        [Authorize(Policy = "PRODUCCION.FICHA.CREAR")]
        public IActionResult Create(Guid? productoId)
        {
            ViewData["ProductoId"] = new SelectList(_context.Productos
                .Where(p => p.EntidadId == CurrentEntidadId && (p.Tipo == "ELABORADO" || p.Tipo == "TERMINADO")), "Id", "Nombre", productoId);

            return View(new FichaCosto {
                VigenteDesde = DateOnly.FromDateTime(DateTime.Now),
                MargenPorcentaje = 20
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "PRODUCCION.FICHA.CREAR")]
        public async Task<IActionResult> Create(FichaCosto fichaCosto)
        {
            if (ModelState.IsValid)
            {
                fichaCosto.EntidadId = CurrentEntidadId;
                fichaCosto.CreadoPor = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

                var result = await _productionService.CreateCostSheetAsync(fichaCosto);
                if (result.Succeeded)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", result.Message);
            }
            ViewData["ProductoId"] = new SelectList(_context.Productos.Where(p => p.EntidadId == CurrentEntidadId), "Id", "Nombre", fichaCosto.ProductoId);
            return View(fichaCosto);
        }

        private bool FichaCostoExists(Guid id)
        {
            return _context.FichaCostos.Any(e => e.Id == id && e.EntidadId == CurrentEntidadId);
        }
    }
}
