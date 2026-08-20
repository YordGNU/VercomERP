using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Controllers
{
    [Authorize(Policy = "NUCLEO.AUDITORIA.VER")]
    public class AuditoriaController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public AuditoriaController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? filterUser, string? filterTable, DateTime? start, DateTime? end)
        {
            var query = _context.Auditoria.AsQueryable();

            if (!string.IsNullOrEmpty(filterUser))
                query = query.Where(a => a.NombreUsuario.Contains(filterUser));

            if (!string.IsNullOrEmpty(filterTable))
                query = query.Where(a => a.EsquemaTabla.Contains(filterTable));

            if (start.HasValue)
                query = query.Where(a => a.OcurridoEn >= start.Value);

            if (end.HasValue)
                query = query.Where(a => a.OcurridoEn <= end.Value);

            var logs = await query.OrderByDescending(a => a.OcurridoEn).Take(500).ToListAsync();
            return View(logs);
        }

        public async Task<IActionResult> Details(long id)
        {
            var entry = await _context.Auditoria.FirstOrDefaultAsync(a => a.Id == id);
            if (entry == null) return NotFound();
            return View(entry);
        }
    }
}
