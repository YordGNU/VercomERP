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
    public class ContratoEconomicoController : Controller
    {
        private readonly AppDbContext _context;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public ContratoEconomicoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ContratoEconomico
        [Authorize(Policy = "COMERCIAL.CONTRATO.VER")]
        public async Task<IActionResult> Index()
        {
            var contratos = await _context.ContratoEconomicos
                .Include(c => c.Cliente)
                .Include(c => c.Proveedor)
                .Where(c => c.EntidadId == CurrentEntidadId)
                .OrderByDescending(c => c.FechaInicio)
                .ToListAsync();
            return View(contratos);
        }

        // GET: ContratoEconomico/Details/5
        [Authorize(Policy = "COMERCIAL.CONTRATO.VER")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var contratoEconomico = await _context.ContratoEconomicos
                .Include(c => c.Cliente)
                .Include(c => c.Proveedor)
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (contratoEconomico == null) return NotFound();

            return View(contratoEconomico);
        }

        // GET: ContratoEconomico/Create
        [Authorize(Policy = "COMERCIAL.CONTRATO.CREAR")]
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.EntidadId == CurrentEntidadId && c.Activo), "Id", "NombreRazonSocial");
            ViewData["ProveedorId"] = new SelectList(_context.Proveedors.Where(p => p.EntidadId == CurrentEntidadId && p.Activo), "Id", "RazonSocial");
            return View(new ContratoEconomico {
                Estado = "VIGENTE",
                FechaInicio = DateOnly.FromDateTime(DateTime.Now),
                TerceroTipo = "CLIENTE"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "COMERCIAL.CONTRATO.CREAR")]
        public async Task<IActionResult> Create(ContratoEconomico contratoEconomico)
        {
            if (ModelState.IsValid)
            {
                contratoEconomico.Id = Guid.NewGuid();
                contratoEconomico.EntidadId = CurrentEntidadId;
                _context.Add(contratoEconomico);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.EntidadId == CurrentEntidadId), "Id", "NombreRazonSocial", contratoEconomico.ClienteId);
            ViewData["ProveedorId"] = new SelectList(_context.Proveedors.Where(p => p.EntidadId == CurrentEntidadId), "Id", "RazonSocial", contratoEconomico.ProveedorId);
            return View(contratoEconomico);
        }

        // GET: ContratoEconomico/Edit/5
        [Authorize(Policy = "COMERCIAL.CONTRATO.CREAR")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var contratoEconomico = await _context.ContratoEconomicos.FirstOrDefaultAsync(c => c.Id == id && c.EntidadId == CurrentEntidadId);
            if (contratoEconomico == null) return NotFound();

            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.EntidadId == CurrentEntidadId), "Id", "NombreRazonSocial", contratoEconomico.ClienteId);
            ViewData["ProveedorId"] = new SelectList(_context.Proveedors.Where(p => p.EntidadId == CurrentEntidadId), "Id", "RazonSocial", contratoEconomico.ProveedorId);
            return View(contratoEconomico);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "COMERCIAL.CONTRATO.CREAR")]
        public async Task<IActionResult> Edit(Guid id, ContratoEconomico contratoEconomico)
        {
            if (id != contratoEconomico.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    contratoEconomico.EntidadId = CurrentEntidadId;
                    _context.Update(contratoEconomico);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContratoEconomicoExists(contratoEconomico.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(contratoEconomico);
        }

        private bool ContratoEconomicoExists(Guid id)
        {
            return _context.ContratoEconomicos.Any(e => e.Id == id && e.EntidadId == CurrentEntidadId);
        }
    }
}
