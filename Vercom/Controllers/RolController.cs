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
    [Authorize(Policy = "SEGURIDAD.ROL.VER")]
    public class RolController : Controller
    {
        private readonly AppDbContext _context;

        public RolController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Rol
        public async Task<IActionResult> Index()
        {
            return View(await _context.Rols.ToListAsync());
        }

        // GET: Rol/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var rol = await _context.Rols
                .Include(r => r.Permisos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rol == null) return NotFound();

            return View(rol);
        }

        // GET: Rol/Create
        [Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
        public async Task<IActionResult> Create([Bind("Codigo,Nombre,Descripcion")] Rol rol)
        {
            if (ModelState.IsValid)
            {
                rol.CreadoEn = DateTime.Now;
                rol.EsSistema = false;
                _context.Add(rol);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rol);
        }

        [HttpGet]
        [Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
        public async Task<IActionResult> ManagePermissions(int id)
        {
            var rol = await _context.Rols.Include(r => r.Permisos).FirstOrDefaultAsync(r => r.Id == id);
            if (rol == null) return NotFound();

            var allPermissions = await _context.Permisos.ToListAsync();
            ViewBag.AllPermissions = allPermissions;

            return View(rol);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
        public async Task<IActionResult> ManagePermissions(int id, int[] selectedPermissions)
        {
            var rol = await _context.Rols.Include(r => r.Permisos).FirstOrDefaultAsync(r => r.Id == id);
            if (rol == null) return NotFound();

            if (rol.EsSistema && rol.Codigo == "ADMINISTRADOR")
            {
                return BadRequest("No se pueden modificar los permisos del rol Administrador de Sistema.");
            }

            rol.Permisos.Clear();
            foreach (var pId in selectedPermissions)
            {
                var permission = await _context.Permisos.FindAsync(pId);
                if (permission != null) rol.Permisos.Add(permission);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Permisos actualizados correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool RolExists(int id)
        {
            return _context.Rols.Any(e => e.Id == id);
        }
    }
}
