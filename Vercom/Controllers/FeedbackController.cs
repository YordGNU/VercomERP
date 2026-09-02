using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Security;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class FeedbackController : Controller
{
    private readonly AppDbContext _context;
    private readonly IEntidadProvider _entidadProvider;
    private readonly INotificationService _notificationService;

    public FeedbackController(AppDbContext context, IEntidadProvider entidadProvider, INotificationService notificationService)
    {
        _context = context;
        _entidadProvider = entidadProvider;
        _notificationService = notificationService;
    }

    [Authorize(Roles = "MASTER,ADMINISTRADOR")]
    public async Task<IActionResult> Index()
    {
        // Solo el usuario master puede ver todo el feedback.
        // Los admins de entidad ven solo el suyo (si aplica) o nada.
        if (!_entidadProvider.IsMaster) return Forbid();

        var feedback = await _context.Feedbacks
            .Include(f => f.Entidad)
            .Include(f => f.Usuario)
            .OrderByDescending(f => f.CreadoEn)
            .ToListAsync();

        return View(feedback);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string tipo, string mensaje, string metadata)
    {
        if (string.IsNullOrEmpty(mensaje)) return BadRequest("El mensaje es obligatorio.");

        var feedback = new Feedback
        {
            Id = Guid.NewGuid(),
            EntidadId = _entidadProvider.CurrentEntidadId,
            UsuarioId = _entidadProvider.CurrentUsuarioId,
            Tipo = tipo,
            Mensaje = mensaje,
            MetadataTecnica = metadata,
            Estado = "PENDIENTE",
            CreadoEn = DateTimeOffset.Now
        };

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();

        // Notificar al Maestro en tiempo real
        await _notificationService.NotifyMasterAsync(
            $"Nuevo Feedback: {tipo}",
            $"{_entidadProvider.CurrentUsuarioId} ha enviado una comunicación.",
            tipo == "ERROR" ? "danger" : "info"
        );

        return Json(new { success = true, message = "Feedback enviado correctamente." });
    }

    [HttpPost]
    [Authorize(Roles = "MASTER,ADMINISTRADOR")]
    public async Task<IActionResult> UpdateStatus(Guid id, string status)
    {
        if (!_entidadProvider.IsMaster) return Forbid();

        var feedback = await _context.Feedbacks.FindAsync(id);
        if (feedback == null) return NotFound();

        feedback.Estado = status;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
