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
    public class AlmacenController : Controller
    {
        private readonly AppDbContext _context;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public AlmacenController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Almacen
        [Authorize(Policy = "INVENTARIO.ALMACEN.VER")]
        public async Task<IActionResult> Index()
        {
            var almacenes = await _context.Almacens
                .Include(a => a.Sucursal)
                .Where(a => a.EntidadId == CurrentEntidadId)
                .OrderBy(a => a.Nombre)
                .ToListAsync();
            return View(almacenes);
        }

        // GET: Almacen/Details/5
        [Authorize(Policy = "INVENTARIO.ALMACEN.VER")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var almacen = await _context.Almacens
                .Include(a => a.Sucursal)
                .Include(a => a.Existencia).ThenInclude(e => e.Producto)
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (almacen == null) return NotFound();

            return View(almacen);
        }

        // GET: Almacen/Create
        [Authorize(Policy = "INVENTARIO.ALMACEN.CREAR")]
        public IActionResult Create()
        {
            ViewData["SucursalId"] = new SelectList(_context.Sucursals.Where(s => s.EntidadId == CurrentEntidadId), "Id", "Nombre");
            return View(new Almacen { Activo = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "INVENTARIO.ALMACEN.CREAR")]
        public async Task<IActionResult> Create(Almacen almacen)
        {
            if (ModelState.IsValid)
            {
                almacen.Id = Guid.NewGuid();
                almacen.EntidadId = CurrentEntidadId;
                _context.Add(almacen);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SucursalId"] = new SelectList(_context.Sucursals.Where(s => s.EntidadId == CurrentEntidadId), "Id", "Nombre", almacen.SucursalId);
            return View(almacen);
        }

        // GET: Almacen/Edit/5
        [Authorize(Policy = "INVENTARIO.ALMACEN.CREAR")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var almacen = await _context.Almacens.FirstOrDefaultAsync(a => a.Id == id && a.EntidadId == CurrentEntidadId);
            if (almacen == null) return NotFound();

            ViewData["SucursalId"] = new SelectList(_context.Sucursals.Where(s => s.EntidadId == CurrentEntidadId), "Id", "Nombre", almacen.SucursalId);
            return View(almacen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "INVENTARIO.ALMACEN.CREAR")]
        public async Task<IActionResult> Edit(Guid id, Almacen almacen)
        {
            if (id != almacen.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    almacen.EntidadId = CurrentEntidadId;
                    _context.Update(almacen);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlmacenExists(almacen.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SucursalId"] = new SelectList(_context.Sucursals.Where(s => s.EntidadId == CurrentEntidadId), "Id", "Nombre", almacen.SucursalId);
            return View(almacen);
        }

        private bool AlmacenExists(Guid id)
        {
            return _context.Almacens.Any(e => e.Id == id && e.EntidadId == CurrentEntidadId);
        }
    }
}
