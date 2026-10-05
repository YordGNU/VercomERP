using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Security;

namespace Vercom.Controllers;

[Authorize]
public sealed class NotificacionController : Controller
{
    private readonly AppDbContext _context;
    private readonly IEntidadProvider _entidadProvider;

    public NotificacionController(AppDbContext context, IEntidadProvider entidadProvider)
    {
        _context = context;
        _entidadProvider = entidadProvider;
    }

    // ============================ FILTRO BANDEJA ============================
    // MASTER             => notificaciones globales (entidad_id NULL y usuario_id NULL).
    // Usuario de entidad => las de SU entidad (entidad_id = CurrentEntidadId)
    //                       + las personales (entidad_id NULL y usuario_id = CurrentUsuarioId).
    private IQueryable<Notificacion> Bandeja()
    {
        IQueryable<Notificacion> q = _context.Notificaciones.AsNoTracking();

        if (_entidadProvider.IsMaster)
            q = q.Where(n => n.EntidadId == null && n.UsuarioId == null);
        else
            q = q.Where(n =>
                n.EntidadId == _entidadProvider.CurrentEntidadId ||
                (n.EntidadId == null && n.UsuarioId == _entidadProvider.CurrentUsuarioId));

        return q;
    }

    // ============================ PÁGINA COMPLETA ============================
    [HttpGet]
    public async Task<IActionResult> Index(string filtro = "todas", string tipo = "todas")
    {
        var q = Bandeja();

        if (filtro == "noleidas") q = q.Where(n => !n.Leida);
        else if (filtro == "leidas") q = q.Where(n => n.Leida);

        if (!string.IsNullOrEmpty(tipo) && tipo != "todas") q = q.Where(n => n.Tipo == tipo);

        var items = await q
            .OrderByDescending(n => n.CreadoEn)
            .Take(300)
            .ToListAsync();

        ViewBag.Filtro = filtro;
        ViewBag.Tipo = tipo;
        ViewBag.Tipos = await _context.Notificaciones.AsNoTracking().Select(n => n.Tipo).Distinct().OrderBy(t => t).ToListAsync();
        ViewBag.NoLeidas = await Bandeja().CountAsync(n => !n.Leida);

        return View(items);
    }

    // ============================ CAMPANA (dropdown + badge) ============================
    [HttpGet]
    public async Task<IActionResult> Ultimas(int n = 5, bool soloNoLeidas = false, string? tipo = null)
    {
        var q = soloNoLeidas ? Bandeja().Where(x => !x.Leida) : Bandeja();
        if (!string.IsNullOrEmpty(tipo) && tipo != "todas")
            q = q.Where(x => x.Tipo == tipo);

        var items = await q
            .OrderByDescending(x => x.CreadoEn)
            .Take(Math.Clamp(n, 1, 20))
            .Select(x => new
            {
                x.Id,
                x.Titulo,
                x.Mensaje,
                x.Tipo,
                x.Leida,
                x.Enlace,
                Tiempo = x.CreadoEn.ToString("HH:mm")
            })
            .ToListAsync();
        return Json(items);
    }

    [HttpGet]
    public async Task<IActionResult> Contador()
    {
        var pendientes = await Bandeja().CountAsync(n => !n.Leida);
        return Json(new { pendientes });
    }

    // ============================ ACCIONES ============================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarLeida(Guid id)
    {
        var notif = await _context.Notificaciones.FirstOrDefaultAsync(n => n.Id == id);
        if (notif is null) return NotFound();
        if (!_entidadProvider.IsMaster && !EsVisible(notif)) return Forbid();

        notif.Leida = true;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarNoLeida(Guid id)
    {
        var notif = await _context.Notificaciones.FirstOrDefaultAsync(n => n.Id == id);
        if (notif is null) return NotFound();
        if (!_entidadProvider.IsMaster && !EsVisible(notif)) return Forbid();

        notif.Leida = false;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarTodasLeidas()
    {
        var q = Bandeja().Where(n => !n.Leida);
        await q.ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true));
        return Ok();
    }

    private bool EsVisible(Notificacion n) =>
        n.EntidadId == _entidadProvider.CurrentEntidadId ||
        (n.EntidadId == null && n.UsuarioId == _entidadProvider.CurrentUsuarioId);
}
