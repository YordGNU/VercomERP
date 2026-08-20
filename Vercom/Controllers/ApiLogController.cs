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
    public class ApiLogController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public ApiLogController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ApiLog
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ApiLogs.Include(a => a.ApiCliente);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ApiLog/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiLog = await _context.ApiLogs
                .Include(a => a.ApiCliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (apiLog == null)
            {
                return NotFound();
            }

            return View(apiLog);
        }

        // GET: ApiLog/Create
        public IActionResult Create()
        {
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id");
            return View();
        }

        // POST: ApiLog/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ApiClienteId,MetodoHttp,Endpoint,CodigoRespuesta,DuracionMs,IpOrigen,OcurridoEn")] ApiLog apiLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(apiLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id", apiLog.ApiClienteId);
            return View(apiLog);
        }

        // GET: ApiLog/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiLog = await _context.ApiLogs.FindAsync(id);
            if (apiLog == null)
            {
                return NotFound();
            }
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id", apiLog.ApiClienteId);
            return View(apiLog);
        }

        // POST: ApiLog/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,ApiClienteId,MetodoHttp,Endpoint,CodigoRespuesta,DuracionMs,IpOrigen,OcurridoEn")] ApiLog apiLog)
        {
            if (id != apiLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(apiLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApiLogExists(apiLog.Id))
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
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id", apiLog.ApiClienteId);
            return View(apiLog);
        }

        // GET: ApiLog/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiLog = await _context.ApiLogs
                .Include(a => a.ApiCliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (apiLog == null)
            {
                return NotFound();
            }

            return View(apiLog);
        }

        // POST: ApiLog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var apiLog = await _context.ApiLogs.FindAsync(id);
            if (apiLog != null)
            {
                _context.ApiLogs.Remove(apiLog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApiLogExists(long id)
        {
            return _context.ApiLogs.Any(e => e.Id == id);
        }
    }
}
