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
    public class WebhookSuscripcionController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public WebhookSuscripcionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: WebhookSuscripcion
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.WebhookSuscripcions.Include(w => w.ApiCliente);
            return View(await appDbContext.ToListAsync());
        }

        // GET: WebhookSuscripcion/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var webhookSuscripcion = await _context.WebhookSuscripcions
                .Include(w => w.ApiCliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (webhookSuscripcion == null)
            {
                return NotFound();
            }

            return View(webhookSuscripcion);
        }

        // GET: WebhookSuscripcion/Create
        public IActionResult Create()
        {
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id");
            return View();
        }

        // POST: WebhookSuscripcion/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ApiClienteId,Evento,UrlDestino,SecretoFirmaHash,Activo,CreadoEn")] WebhookSuscripcion webhookSuscripcion)
        {
            if (ModelState.IsValid)
            {
                webhookSuscripcion.Id = Guid.NewGuid();
                _context.Add(webhookSuscripcion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id", webhookSuscripcion.ApiClienteId);
            return View(webhookSuscripcion);
        }

        // GET: WebhookSuscripcion/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var webhookSuscripcion = await _context.WebhookSuscripcions.FindAsync(id);
            if (webhookSuscripcion == null)
            {
                return NotFound();
            }
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id", webhookSuscripcion.ApiClienteId);
            return View(webhookSuscripcion);
        }

        // POST: WebhookSuscripcion/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ApiClienteId,Evento,UrlDestino,SecretoFirmaHash,Activo,CreadoEn")] WebhookSuscripcion webhookSuscripcion)
        {
            if (id != webhookSuscripcion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(webhookSuscripcion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WebhookSuscripcionExists(webhookSuscripcion.Id))
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
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id", webhookSuscripcion.ApiClienteId);
            return View(webhookSuscripcion);
        }

        // GET: WebhookSuscripcion/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var webhookSuscripcion = await _context.WebhookSuscripcions
                .Include(w => w.ApiCliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (webhookSuscripcion == null)
            {
                return NotFound();
            }

            return View(webhookSuscripcion);
        }

        // POST: WebhookSuscripcion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var webhookSuscripcion = await _context.WebhookSuscripcions.FindAsync(id);
            if (webhookSuscripcion != null)
            {
                _context.WebhookSuscripcions.Remove(webhookSuscripcion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WebhookSuscripcionExists(Guid id)
        {
            return _context.WebhookSuscripcions.Any(e => e.Id == id);
        }
    }
}
