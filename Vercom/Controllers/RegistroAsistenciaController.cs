using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers
{
    [Authorize]
    public class RegistroAsistenciaController : Controller
    {
        private readonly AppDbContext _context;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public RegistroAsistenciaController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "RRHH.ASISTENCIA.VER")]
        public async Task<IActionResult> Index()
        {
            var logs = await _context.RegistroAsistencia
                .Include(a => a.Empleado)
                .Include(a => a.TipoAusencia)
                .Where(a => a.Empleado.EntidadId == CurrentEntidadId)
                .OrderByDescending(a => a.Fecha)
                .Take(100)
                .ToListAsync();
            return View(logs);
        }

        [HttpGet]
        [Authorize(Policy = "RRHH.ASISTENCIA.REGISTRAR")]
        public async Task<IActionResult> Console(DateTime? date)
        {
            var targetDate = DateOnly.FromDateTime(date ?? DateTime.Now);
            var employees = await _context.Empleados
                .Where(e => e.EntidadId == CurrentEntidadId && e.Estado == "ACTIVO")
                .OrderBy(e => e.Apellidos)
                .ToListAsync();

            var records = await _context.RegistroAsistencia
                .Where(a => a.Fecha == targetDate && a.Empleado.EntidadId == CurrentEntidadId)
                .ToDictionaryAsync(a => a.EmpleadoId);

            ViewBag.Date = targetDate;
            ViewBag.TipoAusenciaId = await _context.TipoAusencia.ToListAsync();

            return View(employees.Select(e => new {
                Employee = e,
                Record = records.ContainsKey(e.Id) ? records[e.Id] : null
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RRHH.ASISTENCIA.REGISTRAR")]
        public async Task<IActionResult> SaveConsole(List<RegistroAsistencium> logs, DateTime date)
        {
            var targetDate = DateOnly.FromDateTime(date);

            foreach (var log in logs)
            {
                var existing = await _context.RegistroAsistencia
                    .FirstOrDefaultAsync(a => a.EmpleadoId == log.EmpleadoId && a.Fecha == targetDate);

                if (existing != null)
                {
                    existing.HoraEntrada = log.HoraEntrada;
                    existing.HoraSalida = log.HoraSalida;
                    existing.HorasExtra = log.HorasExtra;
                    existing.TipoAusenciaId = log.TipoAusenciaId;
                    existing.Observaciones = log.Observaciones;
                }
                else
                {
                    log.Id = Guid.NewGuid();
                    log.Fecha = targetDate;
                    log.RegistradoPor = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                    _context.RegistroAsistencia.Add(log);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Asistencia guardada correctamente.";
            return RedirectToAction(nameof(Console), new { date });
        }
    }
}
