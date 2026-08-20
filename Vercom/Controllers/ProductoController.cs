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
    public class ProductoController : Controller
    {
        private readonly AppDbContext _context;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public ProductoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Producto
        [Authorize(Policy = "INVENTARIO.PRODUCTO.VER")]
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos
                .Include(p => p.Familia)
                .Include(p => p.UnidadMedida)
                .Where(p => p.EntidadId == CurrentEntidadId)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
            return View(productos);
        }

        // GET: Producto/Details/5
        [Authorize(Policy = "INVENTARIO.PRODUCTO.VER")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Familia)
                .Include(p => p.UnidadMedida)
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (producto == null) return NotFound();

            return View(producto);
        }

        // GET: Producto/Create
        [Authorize(Policy = "INVENTARIO.PRODUCTO.CREAR")]
        public IActionResult Create()
        {
            PrepareViewBags();
            return View(new Producto { Activo = true, AplicaImpuestoVentas = true, Tipo = "TERMINADO" });
        }

        // POST: Producto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "INVENTARIO.PRODUCTO.CREAR")]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (ModelState.IsValid)
            {
                producto.Id = Guid.NewGuid();
                producto.EntidadId = CurrentEntidadId;
                producto.CreadoEn = DateTimeOffset.Now;
                producto.ActualizadoEn = DateTimeOffset.Now;

                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PrepareViewBags(producto.FamiliaId, producto.UnidadMedidaId);
            return View(producto);
        }

        // GET: Producto/Edit/5
        [Authorize(Policy = "INVENTARIO.PRODUCTO.EDITAR")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == id && p.EntidadId == CurrentEntidadId);
            if (producto == null) return NotFound();

            PrepareViewBags(producto.FamiliaId, producto.UnidadMedidaId);
            return View(producto);
        }

        // POST: Producto/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "INVENTARIO.PRODUCTO.EDITAR")]
        public async Task<IActionResult> Edit(Guid id, Producto producto)
        {
            if (id != producto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    producto.EntidadId = CurrentEntidadId;
                    producto.ActualizadoEn = DateTimeOffset.Now;
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PrepareViewBags(producto.FamiliaId, producto.UnidadMedidaId);
            return View(producto);
        }

        // GET: Producto/Delete/5
        [Authorize(Policy = "INVENTARIO.PRODUCTO.ELIMINAR")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Familia)
                .Include(p => p.UnidadMedida)
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (producto == null) return NotFound();

            // Validar si tiene existencias o movimientos
            var hasStock = await _context.Existencia.AnyAsync(e => e.ProductoId == id && e.Cantidad > 0);
            if (hasStock)
            {
                TempData["Error"] = "No se puede eliminar un producto con existencias en almacén. Debe darle de baja mediante un movimiento.";
                return RedirectToAction(nameof(Index));
            }

            return View(producto);
        }

        // POST: Producto/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "INVENTARIO.PRODUCTO.ELIMINAR")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == id && p.EntidadId == CurrentEntidadId);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private void PrepareViewBags(Guid? familiaId = null, int? unidadId = null)
        {
            ViewData["FamiliaId"] = new SelectList(_context.FamiliaProductos.Where(f => f.EntidadId == CurrentEntidadId), "Id", "Nombre", familiaId);
            ViewData["UnidadMedidaId"] = new SelectList(_context.UnidadMedida, "Id", "Nombre", unidadId);

            var cuentas = _context.CuentaContables
                .Where(c => c.EntidadId == CurrentEntidadId && c.Activo)
                .OrderBy(c => c.Codigo)
                .Select(c => new { c.Id, Display = c.Codigo + " " + c.Nombre })
                .ToList();

            ViewData["CuentaInventarioId"] = new SelectList(cuentas, "Id", "Display");
            ViewData["CuentaCostoVentaId"] = new SelectList(cuentas, "Id", "Display");
            ViewData["CuentaIngresoId"] = new SelectList(cuentas, "Id", "Display");

            ViewBag.Tipos = new SelectList(new[] { "TERMINADO", "INSUMO", "ELABORADO", "SERVICIO" });
        }

        private bool ProductoExists(Guid id)
        {
            return _context.Productos.Any(e => e.Id == id && e.EntidadId == CurrentEntidadId);
        }
    }
}
