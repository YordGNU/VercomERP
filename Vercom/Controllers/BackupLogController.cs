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
    public class BackupLogController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public BackupLogController(AppDbContext context)
        {
            _context = context;
        }

        // GET: BackupLog
        public async Task<IActionResult> Index()
        {
            return View(await _context.BackupLogs.ToListAsync());
        }

        // GET: BackupLog/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var backupLog = await _context.BackupLogs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (backupLog == null)
            {
                return NotFound();
            }

            return View(backupLog);
        }

        // GET: BackupLog/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BackupLog/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Tipo,RutaArchivo,TamanoBytes,Estado,MensajeError,IniciadoEn,FinalizadoEn")] BackupLog backupLog)
        {
            if (ModelState.IsValid)
            {
                backupLog.Id = Guid.NewGuid();
                _context.Add(backupLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(backupLog);
        }

        // GET: BackupLog/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var backupLog = await _context.BackupLogs.FindAsync(id);
            if (backupLog == null)
            {
                return NotFound();
            }
            return View(backupLog);
        }

        // POST: BackupLog/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Tipo,RutaArchivo,TamanoBytes,Estado,MensajeError,IniciadoEn,FinalizadoEn")] BackupLog backupLog)
        {
            if (id != backupLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(backupLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BackupLogExists(backupLog.Id))
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
            return View(backupLog);
        }

        // GET: BackupLog/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var backupLog = await _context.BackupLogs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (backupLog == null)
            {
                return NotFound();
            }

            return View(backupLog);
        }

        // POST: BackupLog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var backupLog = await _context.BackupLogs.FindAsync(id);
            if (backupLog != null)
            {
                _context.BackupLogs.Remove(backupLog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BackupLogExists(Guid id)
        {
            return _context.BackupLogs.Any(e => e.Id == id);
        }
    }
}
