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
    public class AuditoriumController : Controller
    {
        private readonly AppDbContext _context;

        public AuditoriumController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Auditorium
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Auditoria.Include(a => a.Usuario);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Auditorium/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditorium = await _context.Auditoria
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auditorium == null)
            {
                return NotFound();
            }

            return View(auditorium);
        }

        // GET: Auditorium/Create
        public IActionResult Create()
        {
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Id");
            return View();
        }

        // POST: Auditorium/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UsuarioId,NombreUsuario,Accion,EsquemaTabla,RegistroId,ValoresAnteriores,ValoresNuevos,IpOrigen,Canal,DispositivoId,OcurridoEn")] Auditorium auditorium)
        {
            if (ModelState.IsValid)
            {
                _context.Add(auditorium);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Id", auditorium.UsuarioId);
            return View(auditorium);
        }

        // GET: Auditorium/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditorium = await _context.Auditoria.FindAsync(id);
            if (auditorium == null)
            {
                return NotFound();
            }
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Id", auditorium.UsuarioId);
            return View(auditorium);
        }

        // POST: Auditorium/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,UsuarioId,NombreUsuario,Accion,EsquemaTabla,RegistroId,ValoresAnteriores,ValoresNuevos,IpOrigen,Canal,DispositivoId,OcurridoEn")] Auditorium auditorium)
        {
            if (id != auditorium.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(auditorium);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuditoriumExists(auditorium.Id))
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
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Id", auditorium.UsuarioId);
            return View(auditorium);
        }

        // GET: Auditorium/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditorium = await _context.Auditoria
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auditorium == null)
            {
                return NotFound();
            }

            return View(auditorium);
        }

        // POST: Auditorium/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var auditorium = await _context.Auditoria.FindAsync(id);
            if (auditorium != null)
            {
                _context.Auditoria.Remove(auditorium);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuditoriumExists(long id)
        {
            return _context.Auditoria.Any(e => e.Id == id);
        }
    }
}
