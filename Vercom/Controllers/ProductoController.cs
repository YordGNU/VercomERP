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
    public class ProductoController : Controller
    {
        private readonly AppDbContext _context;

        public ProductoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Producto
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Productos.Include(p => p.Familia).Include(p => p.UnidadMedida);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Producto/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Familia)
                .Include(p => p.UnidadMedida)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Producto/Create
        public IActionResult Create()
        {
            ViewData["FamiliaId"] = new SelectList(_context.FamiliaProductos, "Id", "Id");
            ViewData["UnidadMedidaId"] = new SelectList(_context.UnidadMedida, "Id", "Id");
            return View();
        }

        // POST: Producto/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,Codigo,CodigoBarras,Nombre,Descripcion,FamiliaId,UnidadMedidaId,Tipo,CuentaInventarioId,CuentaCostoVentaId,CuentaIngresoId,PrecioVentaActual,AplicaImpuestoVentas,Activo,CreadoEn,ActualizadoEn")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                producto.Id = Guid.NewGuid();
                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FamiliaId"] = new SelectList(_context.FamiliaProductos, "Id", "Id", producto.FamiliaId);
            ViewData["UnidadMedidaId"] = new SelectList(_context.UnidadMedida, "Id", "Id", producto.UnidadMedidaId);
            return View(producto);
        }

        // GET: Producto/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            ViewData["FamiliaId"] = new SelectList(_context.FamiliaProductos, "Id", "Id", producto.FamiliaId);
            ViewData["UnidadMedidaId"] = new SelectList(_context.UnidadMedida, "Id", "Id", producto.UnidadMedidaId);
            return View(producto);
        }

        // POST: Producto/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,Codigo,CodigoBarras,Nombre,Descripcion,FamiliaId,UnidadMedidaId,Tipo,CuentaInventarioId,CuentaCostoVentaId,CuentaIngresoId,PrecioVentaActual,AplicaImpuestoVentas,Activo,CreadoEn,ActualizadoEn")] Producto producto)
        {
            if (id != producto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
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
            ViewData["FamiliaId"] = new SelectList(_context.FamiliaProductos, "Id", "Id", producto.FamiliaId);
            ViewData["UnidadMedidaId"] = new SelectList(_context.UnidadMedida, "Id", "Id", producto.UnidadMedidaId);
            return View(producto);
        }

        // GET: Producto/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Familia)
                .Include(p => p.UnidadMedida)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // POST: Producto/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(Guid id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
