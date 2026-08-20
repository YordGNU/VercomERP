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
    public class DispositivoPoController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public DispositivoPoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DispositivoPo
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.DispositivoPos.Include(d => d.ApiCliente);
            return View(await appDbContext.ToListAsync());
        }

        // GET: DispositivoPo/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dispositivoPo = await _context.DispositivoPos
                .Include(d => d.ApiCliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dispositivoPo == null)
            {
                return NotFound();
            }

            return View(dispositivoPo);
        }

        // GET: DispositivoPo/Create
        public IActionResult Create()
        {
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id");
            return View();
        }

        // POST: DispositivoPo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,SucursalId,AlmacenId,ApiClienteId,Codigo,Nombre,IdentificadorHardware,CajaId,UltimaSincronizacion,VersionAppPos,Estado,CreadoEn")] DispositivoPo dispositivoPo)
        {
            if (ModelState.IsValid)
            {
                dispositivoPo.Id = Guid.NewGuid();
                _context.Add(dispositivoPo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id", dispositivoPo.ApiClienteId);
            return View(dispositivoPo);
        }

        // GET: DispositivoPo/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dispositivoPo = await _context.DispositivoPos.FindAsync(id);
            if (dispositivoPo == null)
            {
                return NotFound();
            }
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id", dispositivoPo.ApiClienteId);
            return View(dispositivoPo);
        }

        // POST: DispositivoPo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,SucursalId,AlmacenId,ApiClienteId,Codigo,Nombre,IdentificadorHardware,CajaId,UltimaSincronizacion,VersionAppPos,Estado,CreadoEn")] DispositivoPo dispositivoPo)
        {
            if (id != dispositivoPo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dispositivoPo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DispositivoPoExists(dispositivoPo.Id))
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
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id", dispositivoPo.ApiClienteId);
            return View(dispositivoPo);
        }

        // GET: DispositivoPo/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dispositivoPo = await _context.DispositivoPos
                .Include(d => d.ApiCliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dispositivoPo == null)
            {
                return NotFound();
            }

            return View(dispositivoPo);
        }

        // POST: DispositivoPo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var dispositivoPo = await _context.DispositivoPos.FindAsync(id);
            if (dispositivoPo != null)
            {
                _context.DispositivoPos.Remove(dispositivoPo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DispositivoPoExists(Guid id)
        {
            return _context.DispositivoPos.Any(e => e.Id == id);
        }
    }
}
