using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Security;

namespace Vercom.Controllers;

[Authorize(Policy = "RRHH.ASISTENCIA.VER")]
public class TurnoTrabajoController : Controller
{
    private readonly AppDbContext _context;
    private readonly IEntidadProvider _entidadProvider;

    public TurnoTrabajoController(AppDbContext context, IEntidadProvider entidadProvider)
    {
        _context = context;
        _entidadProvider = entidadProvider;
    }

    public async Task<IActionResult> Index()
    {
        var entidadId = _entidadProvider.CurrentEntidadId;
        var turnos = await _context.TurnoTrabajos
            .Where(t => t.EntidadId == entidadId)
            .OrderBy(t => t.HoraEntrada)
            .ToListAsync();
        return View(turnos);
    }

    [HttpGet]
    [Authorize(Policy = "RRHH.ASISTENCIA.REGISTRAR")]
    public IActionResult Create()
    {
        return View(new TurnoTrabajo { ToleranciaMinutos = 15, HoraEntrada = new TimeOnly(8, 0), HoraSalida = new TimeOnly(17, 0) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.ASISTENCIA.REGISTRAR")]
    public async Task<IActionResult> Create(TurnoTrabajo turno)
    {
        if (ModelState.IsValid)
        {
            turno.Id = Guid.NewGuid();
            turno.EntidadId = _entidadProvider.CurrentEntidadId;
            turno.CreadoEn = DateTimeOffset.UtcNow;
            _context.TurnoTrabajos.Add(turno);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Turno de trabajo creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        return View(turno);
    }
}
