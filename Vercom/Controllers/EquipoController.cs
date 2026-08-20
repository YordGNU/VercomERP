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
    public class EquipoController : Controller
    {
        private readonly AppDbContext _context;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public EquipoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Equipo
        [Authorize(Policy = "PRODUCCION.FICHA.VER")]
        public async Task<IActionResult> Index()
        {
            var equipos = await _context.Equipos
                .Where(e => e.EntidadId == CurrentEntidadId)
                .OrderBy(e => e.Nombre)
                .ToListAsync();
            return View(equipos);
        }

        // GET: Equipo/Create
        [Authorize(Policy = "PRODUCCION.MANTENIMIENTO.CREAR")]
        public IActionResult Create()
        {
            ViewData["SucursalId"] = new SelectList(_context.Sucursals.Where(s => s.EntidadId == CurrentEntidadId), "Id", "Nombre");
            ViewData["ActivoFijoId"] = new SelectList(_context.ActivoFijos.Where(a => a.EntidadId == CurrentEntidadId), "Id", "Descripcion");
            return View(new Equipo { Estado = "OPERATIVO", FrecuenciaMantenimientoDias = 30 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "PRODUCCION.MANTENIMIENTO.CREAR")]
        public async Task<IActionResult> Create(Equipo equipo)
        {
            if (ModelState.IsValid)
            {
                equipo.Id = Guid.NewGuid();
                equipo.EntidadId = CurrentEntidadId;
                _context.Add(equipo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(equipo);
        }

        private bool EquipoExists(Guid id)
        {
            return _context.Equipos.Any(e => e.Id == id && e.EntidadId == CurrentEntidadId);
        }
    }
}
