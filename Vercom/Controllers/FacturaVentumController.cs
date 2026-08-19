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
    public class FacturaVentumController : Controller
    {
        private readonly AppDbContext _context;

        public FacturaVentumController(AppDbContext context)
        {
            _context = context;
        }

        // GET: FacturaVentum
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.FacturaVenta.Include(f => f.Cliente).Include(f => f.Contrato);
            return View(await appDbContext.ToListAsync());
        }

        // GET: FacturaVentum/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facturaVentum = await _context.FacturaVenta
                .Include(f => f.Cliente)
                .Include(f => f.Contrato)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (facturaVentum == null)
            {
                return NotFound();
            }

            return View(facturaVentum);
        }

        // GET: FacturaVentum/Create
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Id");
            ViewData["ContratoId"] = new SelectList(_context.ContratoEconomicos, "Id", "Id");
            return View();
        }

        // POST: FacturaVentum/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,SucursalId,NumeroFactura,Serie,ClienteId,ContratoId,AlmacenId,CanalVenta,TipoVenta,DispositivoPosId,SesionCajaPosId,Fecha,Subtotal,DescuentoTotal,ImpuestoVentasTotal,Total,Moneda,Estado,MotivoAnulacion,AsientoId,CuentaPorCobrarId,CreadoPor,CreadoEn")] FacturaVentum facturaVentum)
        {
            if (ModelState.IsValid)
            {
                facturaVentum.Id = Guid.NewGuid();
                _context.Add(facturaVentum);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Id", facturaVentum.ClienteId);
            ViewData["ContratoId"] = new SelectList(_context.ContratoEconomicos, "Id", "Id", facturaVentum.ContratoId);
            return View(facturaVentum);
        }

        // GET: FacturaVentum/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facturaVentum = await _context.FacturaVenta.FindAsync(id);
            if (facturaVentum == null)
            {
                return NotFound();
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Id", facturaVentum.ClienteId);
            ViewData["ContratoId"] = new SelectList(_context.ContratoEconomicos, "Id", "Id", facturaVentum.ContratoId);
            return View(facturaVentum);
        }

        // POST: FacturaVentum/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,SucursalId,NumeroFactura,Serie,ClienteId,ContratoId,AlmacenId,CanalVenta,TipoVenta,DispositivoPosId,SesionCajaPosId,Fecha,Subtotal,DescuentoTotal,ImpuestoVentasTotal,Total,Moneda,Estado,MotivoAnulacion,AsientoId,CuentaPorCobrarId,CreadoPor,CreadoEn")] FacturaVentum facturaVentum)
        {
            if (id != facturaVentum.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(facturaVentum);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacturaVentumExists(facturaVentum.Id))
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
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Id", facturaVentum.ClienteId);
            ViewData["ContratoId"] = new SelectList(_context.ContratoEconomicos, "Id", "Id", facturaVentum.ContratoId);
            return View(facturaVentum);
        }

        // GET: FacturaVentum/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facturaVentum = await _context.FacturaVenta
                .Include(f => f.Cliente)
                .Include(f => f.Contrato)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (facturaVentum == null)
            {
                return NotFound();
            }

            return View(facturaVentum);
        }

        // POST: FacturaVentum/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var facturaVentum = await _context.FacturaVenta.FindAsync(id);
            if (facturaVentum != null)
            {
                _context.FacturaVenta.Remove(facturaVentum);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FacturaVentumExists(Guid id)
        {
            return _context.FacturaVenta.Any(e => e.Id == id);
        }
    }
}
