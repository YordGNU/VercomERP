using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Controllers
{
    [Authorize]
    public class CuentaPorPagarController : Controller
    {
        private readonly AppDbContext _context;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public CuentaPorPagarController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CuentaPorPagar
        [Authorize(Policy = "CONTABILIDAD.CXP.VER")]
        public async Task<IActionResult> Index()
        {
            var cxp = await _context.CuentaPorPagars              
                .Where(c => c.EntidadId == CurrentEntidadId)
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();
            return View(cxp);
        }

        // GET: CuentaPorPagar/Details/5
        [Authorize(Policy = "CONTABILIDAD.CXP.VER")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var cxp = await _context.CuentaPorPagars                
                .Include(c => c.AsientoOrigen)
                .Include(c => c.PagoAplicados)
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (cxp == null) return NotFound();

            return View(cxp);
        }
    }
}
