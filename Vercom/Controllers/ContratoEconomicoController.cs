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
    public class ContratoEconomicoController : Controller
    {
        private readonly AppDbContext _context;

        public ContratoEconomicoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ContratoEconomico
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ContratoEconomicos.Include(c => c.Cliente).Include(c => c.Proveedor);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ContratoEconomico/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contratoEconomico = await _context.ContratoEconomicos
                .Include(c => c.Cliente)
                .Include(c => c.Proveedor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contratoEconomico == null)
            {
                return NotFound();
            }

            return View(contratoEconomico);
        }

        // GET: ContratoEconomico/Create
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Id");
            ViewData["ProveedorId"] = new SelectList(_context.Proveedors, "Id", "Id");
            return View();
        }

        // POST: ContratoEconomico/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,TerceroTipo,ClienteId,ProveedorId,NumeroContrato,Objeto,FechaFirma,FechaInicio,FechaFin,MontoTotal,DocumentoUrl,Estado")] ContratoEconomico contratoEconomico)
        {
            if (ModelState.IsValid)
            {
                contratoEconomico.Id = Guid.NewGuid();
                _context.Add(contratoEconomico);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Id", contratoEconomico.ClienteId);
            ViewData["ProveedorId"] = new SelectList(_context.Proveedors, "Id", "Id", contratoEconomico.ProveedorId);
            return View(contratoEconomico);
        }

        // GET: ContratoEconomico/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contratoEconomico = await _context.ContratoEconomicos.FindAsync(id);
            if (contratoEconomico == null)
            {
                return NotFound();
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Id", contratoEconomico.ClienteId);
            ViewData["ProveedorId"] = new SelectList(_context.Proveedors, "Id", "Id", contratoEconomico.ProveedorId);
            return View(contratoEconomico);
        }

        // POST: ContratoEconomico/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,TerceroTipo,ClienteId,ProveedorId,NumeroContrato,Objeto,FechaFirma,FechaInicio,FechaFin,MontoTotal,DocumentoUrl,Estado")] ContratoEconomico contratoEconomico)
        {
            if (id != contratoEconomico.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contratoEconomico);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContratoEconomicoExists(contratoEconomico.Id))
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
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Id", contratoEconomico.ClienteId);
            ViewData["ProveedorId"] = new SelectList(_context.Proveedors, "Id", "Id", contratoEconomico.ProveedorId);
            return View(contratoEconomico);
        }

        // GET: ContratoEconomico/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contratoEconomico = await _context.ContratoEconomicos
                .Include(c => c.Cliente)
                .Include(c => c.Proveedor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contratoEconomico == null)
            {
                return NotFound();
            }

            return View(contratoEconomico);
        }

        // POST: ContratoEconomico/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var contratoEconomico = await _context.ContratoEconomicos.FindAsync(id);
            if (contratoEconomico != null)
            {
                _context.ContratoEconomicos.Remove(contratoEconomico);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContratoEconomicoExists(Guid id)
        {
            return _context.ContratoEconomicos.Any(e => e.Id == id);
        }
    }
}
