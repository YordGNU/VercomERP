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
    public class ClienteController : Controller
    {
        private readonly AppDbContext _context;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public ClienteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Cliente
        [Authorize(Policy = "COMERCIAL.CLIENTE.VER")]
        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Clientes
                .Where(c => c.EntidadId == CurrentEntidadId)
                .OrderBy(c => c.NombreRazonSocial)
                .ToListAsync();
            return View(clientes);
        }

        // GET: Cliente/Details/5
        [Authorize(Policy = "COMERCIAL.CLIENTE.VER")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes
                .Include(c => c.ContratoEconomicos)
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // GET: Cliente/Create
        [Authorize(Policy = "COMERCIAL.CLIENTE.CREAR")]
        public IActionResult Create()
        {
            ViewData["ListaPrecioId"] = new SelectList(_context.ListaPrecios.Where(l => l.EntidadId == CurrentEntidadId), "Id", "Nombre");
            return View(new Cliente { Activo = true, TipoPersona = "JURIDICA", Segmento = "MINORISTA" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "COMERCIAL.CLIENTE.CREAR")]
        public async Task<IActionResult> Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                cliente.Id = Guid.NewGuid();
                cliente.EntidadId = CurrentEntidadId;
                cliente.CreadoEn = DateTimeOffset.Now;
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ListaPrecioId"] = new SelectList(_context.ListaPrecios.Where(l => l.EntidadId == CurrentEntidadId), "Id", "Nombre", cliente.ListaPrecioId);
            return View(cliente);
        }

        // GET: Cliente/Edit/5
        [Authorize(Policy = "COMERCIAL.CLIENTE.EDITAR")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.EntidadId == CurrentEntidadId);
            if (cliente == null) return NotFound();

            ViewData["ListaPrecioId"] = new SelectList(_context.ListaPrecios.Where(l => l.EntidadId == CurrentEntidadId), "Id", "Nombre", cliente.ListaPrecioId);
            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "COMERCIAL.CLIENTE.EDITAR")]
        public async Task<IActionResult> Edit(Guid id, Cliente cliente)
        {
            if (id != cliente.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    cliente.EntidadId = CurrentEntidadId;
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        private bool ClienteExists(Guid id)
        {
            return _context.Clientes.Any(e => e.Id == id && e.EntidadId == CurrentEntidadId);
        }
    }
}
