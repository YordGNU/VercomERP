using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Controllers
{
    public class ApiRateLimitController : Controller
    {
        private readonly AppDbContext _context;

        public ApiRateLimitController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ApiRateLimit
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ApiRateLimits.Include(a => a.ApiCliente);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ApiRateLimit/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiRateLimit = await _context.ApiRateLimits
                .Include(a => a.ApiCliente)
                .FirstOrDefaultAsync(m => m.ApiClienteId == id);
            if (apiRateLimit == null)
            {
                return NotFound();
            }

            return View(apiRateLimit);
        }

        // GET: ApiRateLimit/Create
        public IActionResult Create()
        {
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id");
            return View();
        }

        // POST: ApiRateLimit/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ApiClienteId,SolicitudesPorMinuto,ActualizadoEn")] ApiRateLimit apiRateLimit)
        {
            if (ModelState.IsValid)
            {
                apiRateLimit.ApiClienteId = Guid.NewGuid();
                _context.Add(apiRateLimit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id", apiRateLimit.ApiClienteId);
            return View(apiRateLimit);
        }

        // GET: ApiRateLimit/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiRateLimit = await _context.ApiRateLimits.FindAsync(id);
            if (apiRateLimit == null)
            {
                return NotFound();
            }
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id", apiRateLimit.ApiClienteId);
            return View(apiRateLimit);
        }

        // POST: ApiRateLimit/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ApiClienteId,SolicitudesPorMinuto,ActualizadoEn")] ApiRateLimit apiRateLimit)
        {
            if (id != apiRateLimit.ApiClienteId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(apiRateLimit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApiRateLimitExists(apiRateLimit.ApiClienteId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id", apiRateLimit.ApiClienteId);
            return View(apiRateLimit);
        }

        // GET: ApiRateLimit/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiRateLimit = await _context.ApiRateLimits
                .Include(a => a.ApiCliente)
                .FirstOrDefaultAsync(m => m.ApiClienteId == id);
            if (apiRateLimit == null)
            {
                return NotFound();
            }

            return View(apiRateLimit);
        }

        // POST: ApiRateLimit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var apiRateLimit = await _context.ApiRateLimits.FindAsync(id);
            if (apiRateLimit != null)
            {
                _context.ApiRateLimits.Remove(apiRateLimit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApiRateLimitExists(Guid id)
        {
            return _context.ApiRateLimits.Any(e => e.ApiClienteId == id);
        }
    }
}
