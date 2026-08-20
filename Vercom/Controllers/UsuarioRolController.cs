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
    public class UsuarioRolController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public UsuarioRolController(AppDbContext context)
        {
            _context = context;
        }

        // GET: UsuarioRol
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.UsuarioRols.Include(u => u.AsignadoPorNavigation).Include(u => u.Rol).Include(u => u.Sucursal).Include(u => u.Usuario);
            return View(await appDbContext.ToListAsync());
        }

        // GET: UsuarioRol/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarioRol = await _context.UsuarioRols
                .Include(u => u.AsignadoPorNavigation)
                .Include(u => u.Rol)
                .Include(u => u.Sucursal)
                .Include(u => u.Usuario)
                .FirstOrDefaultAsync(m => m.UsuarioId == id);
            if (usuarioRol == null)
            {
                return NotFound();
            }

            return View(usuarioRol);
        }

        // GET: UsuarioRol/Create
        public IActionResult Create()
        {
            ViewData["AsignadoPor"] = new SelectList(_context.Usuarios, "Id", "Id");
            ViewData["RolId"] = new SelectList(_context.Rols, "Id", "Id");
            ViewData["SucursalId"] = new SelectList(_context.Sucursals, "Id", "Id");
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Id");
            return View();
        }

        // POST: UsuarioRol/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UsuarioId,RolId,SucursalId,AsignadoEn,AsignadoPor")] UsuarioRol usuarioRol)
        {
            if (ModelState.IsValid)
            {
                usuarioRol.UsuarioId = Guid.NewGuid();
                _context.Add(usuarioRol);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AsignadoPor"] = new SelectList(_context.Usuarios, "Id", "Id", usuarioRol.AsignadoPor);
            ViewData["RolId"] = new SelectList(_context.Rols, "Id", "Id", usuarioRol.RolId);
            ViewData["SucursalId"] = new SelectList(_context.Sucursals, "Id", "Id", usuarioRol.SucursalId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Id", usuarioRol.UsuarioId);
            return View(usuarioRol);
        }

        // GET: UsuarioRol/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarioRol = await _context.UsuarioRols.FindAsync(id);
            if (usuarioRol == null)
            {
                return NotFound();
            }
            ViewData["AsignadoPor"] = new SelectList(_context.Usuarios, "Id", "Id", usuarioRol.AsignadoPor);
            ViewData["RolId"] = new SelectList(_context.Rols, "Id", "Id", usuarioRol.RolId);
            ViewData["SucursalId"] = new SelectList(_context.Sucursals, "Id", "Id", usuarioRol.SucursalId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Id", usuarioRol.UsuarioId);
            return View(usuarioRol);
        }

        // POST: UsuarioRol/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("UsuarioId,RolId,SucursalId,AsignadoEn,AsignadoPor")] UsuarioRol usuarioRol)
        {
            if (id != usuarioRol.UsuarioId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuarioRol);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioRolExists(usuarioRol.UsuarioId))
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
            ViewData["AsignadoPor"] = new SelectList(_context.Usuarios, "Id", "Id", usuarioRol.AsignadoPor);
            ViewData["RolId"] = new SelectList(_context.Rols, "Id", "Id", usuarioRol.RolId);
            ViewData["SucursalId"] = new SelectList(_context.Sucursals, "Id", "Id", usuarioRol.SucursalId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Id", usuarioRol.UsuarioId);
            return View(usuarioRol);
        }

        // GET: UsuarioRol/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarioRol = await _context.UsuarioRols
                .Include(u => u.AsignadoPorNavigation)
                .Include(u => u.Rol)
                .Include(u => u.Sucursal)
                .Include(u => u.Usuario)
                .FirstOrDefaultAsync(m => m.UsuarioId == id);
            if (usuarioRol == null)
            {
                return NotFound();
            }

            return View(usuarioRol);
        }

        // POST: UsuarioRol/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var usuarioRol = await _context.UsuarioRols.FindAsync(id);
            if (usuarioRol != null)
            {
                _context.UsuarioRols.Remove(usuarioRol);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioRolExists(Guid id)
        {
            return _context.UsuarioRols.Any(e => e.UsuarioId == id);
        }
    }
}
