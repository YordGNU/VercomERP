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
    public class CuentaPorCobrarController : Controller
    {
        private readonly AppDbContext _context;
        private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public CuentaPorCobrarController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CuentaPorCobrar
        [Authorize(Policy = "CONTABILIDAD.CXC.VER")]
        public async Task<IActionResult> Index()
        {
            var cxc = await _context.CuentaPorCobrars
                .Where(c => c.EntidadId == CurrentEntidadId)
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();
            return View(cxc);
        }

        // GET: CuentaPorCobrar/Details/5
        [Authorize(Policy = "CONTABILIDAD.CXC.VER")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var cxc = await _context.CuentaPorCobrars               
                .Include(c => c.AsientoOrigen)
                .Include(c => c.PagoAplicados)
                .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == CurrentEntidadId);

            if (cxc == null) return NotFound();

            return View(cxc);
        }
    }
}
