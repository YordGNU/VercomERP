using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers
{
    [Authorize]
    public class DeclaracionJuradumController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ITaxService _taxService;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public DeclaracionJuradumController(AppDbContext context, ITaxService taxService)
        {
            _context = context;
            _taxService = taxService;
        }

        // GET: DeclaracionJuradum
        [Authorize(Policy = "ACC_VIEW_PLAN")]
        public async Task<IActionResult> Index()
        {
            var declaraciones = await _context.DeclaracionJurada
                .Include(d => d.Periodo)
                .Include(d => d.TipoObligacion)
                .Where(d => d.EntidadId == CurrentEntidadId)
                .OrderByDescending(d => d.CreadoEn)
                .ToListAsync();
            return View(declaraciones);
        }

        [Authorize(Policy = "ACC_VIEW_PLAN")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var entry = await _context.DeclaracionJurada
                .Include(d => d.Periodo)
                .Include(d => d.TipoObligacion)
                .Include(d => d.Asiento)
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (entry == null) return NotFound();
            return View(entry);
        }

        [HttpGet]
        [Authorize(Policy = "CONTABILIDAD.DECLARACION.CREAR")]
        public IActionResult Create()
        {
            var periodos = _context.PeriodoContables
                .Where(p => p.EntidadId == CurrentEntidadId)
                .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes)
                .Select(p => new { p.Id, Display = p.Mes + " / " + p.Anio })
                .ToList();

            ViewData["PeriodoId"] = new SelectList(periodos, "Id", "Display");
            ViewData["TipoObligacionId"] = new SelectList(_context.TipoObligacionFiscals, "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CONTABILIDAD.DECLARACION.CREAR")]
        public async Task<IActionResult> Create(int tipoObligacionId, Guid periodoId)
        {
            var result = await _taxService.GenerateTaxDeclarationAsync(CurrentEntidadId, tipoObligacionId, periodoId);

            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result.Message);
            var periodos = _context.PeriodoContables
                .Where(p => p.EntidadId == CurrentEntidadId)
                .Select(p => new { p.Id, Display = p.Mes + " / " + p.Anio })
                .ToList();

            ViewData["PeriodoId"] = new SelectList(periodos, "Id", "Display", periodoId);
            ViewData["TipoObligacionId"] = new SelectList(_context.TipoObligacionFiscals, "Id", "Nombre", tipoObligacionId);
            return View();
        }

        private bool DeclaracionJuradumExists(Guid id)
        {
            return _context.DeclaracionJurada.Any(e => e.Id == id && e.EntidadId == CurrentEntidadId);
        }
    }
}
