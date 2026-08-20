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
    public class PosSyncLogController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public PosSyncLogController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PosSyncLog
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PosSyncLogs.Include(p => p.DispositivoPos);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PosSyncLog/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posSyncLog = await _context.PosSyncLogs
                .Include(p => p.DispositivoPos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (posSyncLog == null)
            {
                return NotFound();
            }

            return View(posSyncLog);
        }

        // GET: PosSyncLog/Create
        public IActionResult Create()
        {
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id");
            return View();
        }

        // POST: PosSyncLog/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DispositivoPosId,TipoSync,Direccion,RegistrosProcesados,Estado,DetalleError,IniciadoEn,FinalizadoEn")] PosSyncLog posSyncLog)
        {
            if (ModelState.IsValid)
            {
                posSyncLog.Id = Guid.NewGuid();
                _context.Add(posSyncLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id", posSyncLog.DispositivoPosId);
            return View(posSyncLog);
        }

        // GET: PosSyncLog/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posSyncLog = await _context.PosSyncLogs.FindAsync(id);
            if (posSyncLog == null)
            {
                return NotFound();
            }
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id", posSyncLog.DispositivoPosId);
            return View(posSyncLog);
        }

        // POST: PosSyncLog/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,DispositivoPosId,TipoSync,Direccion,RegistrosProcesados,Estado,DetalleError,IniciadoEn,FinalizadoEn")] PosSyncLog posSyncLog)
        {
            if (id != posSyncLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(posSyncLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PosSyncLogExists(posSyncLog.Id))
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
            ViewData["DispositivoPosId"] = new SelectList(_context.DispositivoPos, "Id", "Id", posSyncLog.DispositivoPosId);
            return View(posSyncLog);
        }

        // GET: PosSyncLog/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posSyncLog = await _context.PosSyncLogs
                .Include(p => p.DispositivoPos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (posSyncLog == null)
            {
                return NotFound();
            }

            return View(posSyncLog);
        }

        // POST: PosSyncLog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var posSyncLog = await _context.PosSyncLogs.FindAsync(id);
            if (posSyncLog != null)
            {
                _context.PosSyncLogs.Remove(posSyncLog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PosSyncLogExists(Guid id)
        {
            return _context.PosSyncLogs.Any(e => e.Id == id);
        }
    }
}
