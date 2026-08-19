using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public UsuarioController(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        // GET: Usuario
        [Authorize(Policy = "SEC_VIEW_USERS")]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.EsEmpleado)
                .Include(u => u.Sucursal)
                .Where(u => u.EntidadId == CurrentEntidadId)
                .ToListAsync();
            return View(usuarios);
        }

        // GET: Usuario/Details/5
        [Authorize(Policy = "SEC_VIEW_USERS")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.EsEmpleado)
                .Include(u => u.Sucursal)
                .Include(u => u.UsuarioRolUsuarios).ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // GET: Usuario/Create
        [Authorize(Policy = "SEC_EDIT_USERS")]
        public IActionResult Create()
        {
            ViewData["EsEmpleadoId"] = new SelectList(_context.Empleados.Where(e => e.EntidadId == CurrentEntidadId), "Id", "NombreCompleto");
            ViewData["SucursalId"] = new SelectList(_context.Sucursals.Where(s => s.EntidadId == CurrentEntidadId), "Id", "Nombre");
            return View();
        }

        // POST: Usuario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SEC_EDIT_USERS")]
        public async Task<IActionResult> Create(Usuario usuario, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("HashPassword", "La contraseña es requerida.");
            }

            if (ModelState.IsValid)
            {
                usuario.Id = Guid.NewGuid();
                usuario.EntidadId = CurrentEntidadId;
                usuario.HashPassword = _authService.HashPassword(password);
                usuario.CreadoEn = DateTimeOffset.Now;
                usuario.ActualizadoEn = DateTimeOffset.Now;
                usuario.DebeCambiarPass = true;

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EsEmpleadoId"] = new SelectList(_context.Empleados.Where(e => e.EntidadId == CurrentEntidadId), "Id", "NombreCompleto", usuario.EsEmpleadoId);
            ViewData["SucursalId"] = new SelectList(_context.Sucursals.Where(s => s.EntidadId == CurrentEntidadId), "Id", "Nombre", usuario.SucursalId);
            return View(usuario);
        }

        // GET: Usuario/Edit/5
        [Authorize(Policy = "SEC_EDIT_USERS")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.EntidadId == CurrentEntidadId);
            if (usuario == null) return NotFound();

            ViewData["EsEmpleadoId"] = new SelectList(_context.Empleados.Where(e => e.EntidadId == CurrentEntidadId), "Id", "NombreCompleto", usuario.EsEmpleadoId);
            ViewData["SucursalId"] = new SelectList(_context.Sucursals.Where(s => s.EntidadId == CurrentEntidadId), "Id", "Nombre", usuario.SucursalId);
            return View(usuario);
        }

        // POST: Usuario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SEC_EDIT_USERS")]
        public async Task<IActionResult> Edit(Guid id, Usuario usuario)
        {
            if (id != usuario.Id) return NotFound();

            var dbUser = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id && u.EntidadId == CurrentEntidadId);
            if (dbUser == null) return NotFound();

            if (ModelState.IsValid)
            {
                usuario.EntidadId = CurrentEntidadId;
                usuario.HashPassword = dbUser.HashPassword; // No permitir editar hash desde aquí
                usuario.ActualizadoEn = DateTimeOffset.Now;

                _context.Update(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EsEmpleadoId"] = new SelectList(_context.Empleados.Where(e => e.EntidadId == CurrentEntidadId), "Id", "NombreCompleto", usuario.EsEmpleadoId);
            ViewData["SucursalId"] = new SelectList(_context.Sucursals.Where(s => s.EntidadId == CurrentEntidadId), "Id", "Nombre", usuario.SucursalId);
            return View(usuario);
        }

        [HttpPost]
        [Authorize(Policy = "SEC_EDIT_USERS")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.EntidadId == CurrentEntidadId);
            if (user == null) return NotFound();

            user.Activo = !user.Activo;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Policy = "SEC_EDIT_USERS")]
        public async Task<IActionResult> ResetPassword(Guid id, string newPassword)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.EntidadId == CurrentEntidadId);
            if (user == null) return NotFound();

            user.HashPassword = _authService.HashPassword(newPassword);
            user.DebeCambiarPass = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Contraseña de {user.NombreUsuario} reiniciada.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private bool UsuarioExists(Guid id)
        {
            return _context.Usuarios.Any(e => e.Id == id && e.EntidadId == CurrentEntidadId);
        }
    }
}
