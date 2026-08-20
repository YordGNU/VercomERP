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
    public class WebhookEntregaController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public WebhookEntregaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: WebhookEntrega
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.WebhookEntregas.Include(w => w.Suscripcion);
            return View(await appDbContext.ToListAsync());
        }

        // GET: WebhookEntrega/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var webhookEntrega = await _context.WebhookEntregas
                .Include(w => w.Suscripcion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (webhookEntrega == null)
            {
                return NotFound();
            }

            return View(webhookEntrega);
        }

        // GET: WebhookEntrega/Create
        public IActionResult Create()
        {
            ViewData["SuscripcionId"] = new SelectList(_context.WebhookSuscripcions, "Id", "Id");
            return View();
        }

        // POST: WebhookEntrega/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SuscripcionId,PayloadJson,IntentoNumero,CodigoRespuestaHttp,Exitoso,ProximoReintentoEn,EnviadoEn")] WebhookEntrega webhookEntrega)
        {
            if (ModelState.IsValid)
            {
                webhookEntrega.Id = Guid.NewGuid();
                _context.Add(webhookEntrega);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SuscripcionId"] = new SelectList(_context.WebhookSuscripcions, "Id", "Id", webhookEntrega.SuscripcionId);
            return View(webhookEntrega);
        }

        // GET: WebhookEntrega/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var webhookEntrega = await _context.WebhookEntregas.FindAsync(id);
            if (webhookEntrega == null)
            {
                return NotFound();
            }
            ViewData["SuscripcionId"] = new SelectList(_context.WebhookSuscripcions, "Id", "Id", webhookEntrega.SuscripcionId);
            return View(webhookEntrega);
        }

        // POST: WebhookEntrega/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,SuscripcionId,PayloadJson,IntentoNumero,CodigoRespuestaHttp,Exitoso,ProximoReintentoEn,EnviadoEn")] WebhookEntrega webhookEntrega)
        {
            if (id != webhookEntrega.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(webhookEntrega);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WebhookEntregaExists(webhookEntrega.Id))
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
            ViewData["SuscripcionId"] = new SelectList(_context.WebhookSuscripcions, "Id", "Id", webhookEntrega.SuscripcionId);
            return View(webhookEntrega);
        }

        // GET: WebhookEntrega/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var webhookEntrega = await _context.WebhookEntregas
                .Include(w => w.Suscripcion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (webhookEntrega == null)
            {
                return NotFound();
            }

            return View(webhookEntrega);
        }

        // POST: WebhookEntrega/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var webhookEntrega = await _context.WebhookEntregas.FindAsync(id);
            if (webhookEntrega != null)
            {
                _context.WebhookEntregas.Remove(webhookEntrega);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WebhookEntregaExists(Guid id)
        {
            return _context.WebhookEntregas.Any(e => e.Id == id);
        }
    }
}
