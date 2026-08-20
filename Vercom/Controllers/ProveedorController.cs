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
    public class ProveedorController : Controller
    {
        private readonly AppDbContext _context;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public ProveedorController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Proveedor
        [Authorize(Policy = "COMERCIAL.PROVEEDOR.VER")]
        public async Task<IActionResult> Index()
        {
            var proveedores = await _context.Proveedors
                .Where(p => p.EntidadId == CurrentEntidadId)
                .OrderBy(p => p.RazonSocial)
                .ToListAsync();
            return View(proveedores);
        }

        // GET: Proveedor/Details/5
        [Authorize(Policy = "COMERCIAL.PROVEEDOR.VER")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var proveedor = await _context.Proveedors
                //.Include(p => p.ContratoEconomicos)
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (proveedor == null) return NotFound();

            return View(proveedor);
        }

        // GET: Proveedor/Create
        [Authorize(Policy = "COMERCIAL.PROVEEDOR.CREAR")]
        public IActionResult Create()
        {
            return View(new Proveedor { Activo = true, TipoPersona = "JURIDICA" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "COMERCIAL.PROVEEDOR.CREAR")]
        public async Task<IActionResult> Create(Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
                proveedor.Id = Guid.NewGuid();
                proveedor.EntidadId = CurrentEntidadId;
                proveedor.CreadoEn = DateTimeOffset.Now;
                _context.Add(proveedor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(proveedor);
        }

        // GET: Proveedor/Edit/5
        [Authorize(Policy = "COMERCIAL.PROVEEDOR.EDITAR")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var proveedor = await _context.Proveedors.FirstOrDefaultAsync(p => p.Id == id && p.EntidadId == CurrentEntidadId);
            if (proveedor == null) return NotFound();

            return View(proveedor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "COMERCIAL.PROVEEDOR.EDITAR")]
        public async Task<IActionResult> Edit(Guid id, Proveedor proveedor)
        {
            if (id != proveedor.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    proveedor.EntidadId = CurrentEntidadId;
                    _context.Update(proveedor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProveedorExists(proveedor.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(proveedor);
        }

        private bool ProveedorExists(Guid id)
        {
            return _context.Proveedors.Any(e => e.Id == id && e.EntidadId == CurrentEntidadId);
        }
    }
}
