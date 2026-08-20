using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers
{
    [Authorize]
    public class EmpleadoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHRService _hrService;

        public EmpleadoController(AppDbContext context, IHRService hrService)
        {
            _context = context;
            _hrService = hrService;
        }

        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        // GET: Empleado
        [Authorize(Policy = "RRHH.EMPLEADO.VER")]
        public async Task<IActionResult> Index()
        {
            var empleados = await _context.Empleados
                .Include(e => e.Cargo)
                .Where(e => e.EntidadId == CurrentEntidadId)
                .OrderBy(e => e.Apellidos)
                .ToListAsync();
            return View(empleados);
        }

        // GET: Empleado/File/5 (Expediente Digital)
        [Authorize(Policy = "RRHH.EMPLEADO.VER")]
        public async Task<IActionResult> File(Guid? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados
                .Include(e => e.Cargo)
                .Include(e => e.ContratoLaborals)
                .Include(e => e.RegistroAsistencia).ThenInclude(a => a.TipoAusencia)
                .Include(e => e.SaldoVacaciones)
                .Include(e => e.CertificadoMedicos)
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (empleado == null) return NotFound();

            ViewBag.AccumulatedVacations = await _hrService.GetAccumulatedVacationsAsync(empleado.Id);
            return View(empleado);
        }

        // GET: Empleado/Create
        [Authorize(Policy = "RRHH.EMPLEADO.CREAR")]
        public IActionResult Create()
        {
            ViewData["CargoId"] = new SelectList(_context.Cargos.Where(c => c.EntidadId == CurrentEntidadId), "Id", "Nombre");
            ViewData["SucursalId"] = new SelectList(_context.Sucursals.Where(s => s.EntidadId == CurrentEntidadId), "Id", "Nombre");
            return View(new Empleado { Estado = "ACTIVO", FechaIngreso = DateOnly.FromDateTime(DateTime.Now) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RRHH.EMPLEADO.CREAR")]
        public async Task<IActionResult> Create(Empleado empleado)
        {
            if (ModelState.IsValid)
            {
                empleado.Id = Guid.NewGuid();
                empleado.EntidadId = CurrentEntidadId;
                empleado.CreadoEn = DateTimeOffset.Now;
                _context.Add(empleado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CargoId"] = new SelectList(_context.Cargos.Where(c => c.EntidadId == CurrentEntidadId), "Id", "Nombre", empleado.CargoId);
            return View(empleado);
        }

        // GET: Empleado/Edit/5
        [Authorize(Policy = "RRHH.EMPLEADO.EDITAR")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.Id == id && e.EntidadId == CurrentEntidadId);
            if (empleado == null) return NotFound();

            ViewData["CargoId"] = new SelectList(_context.Cargos.Where(c => c.EntidadId == CurrentEntidadId), "Id", "Nombre", empleado.CargoId);
            ViewData["SucursalId"] = new SelectList(_context.Sucursals.Where(s => s.EntidadId == CurrentEntidadId), "Id", "Nombre", empleado.SucursalId);
            return View(empleado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RRHH.EMPLEADO.EDITAR")]
        public async Task<IActionResult> Edit(Guid id, Empleado empleado)
        {
            if (id != empleado.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    empleado.EntidadId = CurrentEntidadId;
                    _context.Update(empleado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpleadoExists(empleado.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(empleado);
        }

        private bool EmpleadoExists(Guid id)
        {
            return _context.Empleados.Any(e => e.Id == id && e.EntidadId == CurrentEntidadId);
        }
    }
}
