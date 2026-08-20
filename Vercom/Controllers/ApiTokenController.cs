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
    public class ApiTokenController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());

        public ApiTokenController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ApiToken
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ApiTokens.Include(a => a.ApiCliente);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ApiToken/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiToken = await _context.ApiTokens
                .Include(a => a.ApiCliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (apiToken == null)
            {
                return NotFound();
            }

            return View(apiToken);
        }

        // GET: ApiToken/Create
        public IActionResult Create()
        {
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id");
            return View();
        }

        // POST: ApiToken/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ApiClienteId,TokenHash,Tipo,EmitidoEn,ExpiraEn,Revocado,IpOrigen")] ApiToken apiToken)
        {
            if (ModelState.IsValid)
            {
                apiToken.Id = Guid.NewGuid();
                _context.Add(apiToken);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id", apiToken.ApiClienteId);
            return View(apiToken);
        }

        // GET: ApiToken/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiToken = await _context.ApiTokens.FindAsync(id);
            if (apiToken == null)
            {
                return NotFound();
            }
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id", apiToken.ApiClienteId);
            return View(apiToken);
        }

        // POST: ApiToken/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ApiClienteId,TokenHash,Tipo,EmitidoEn,ExpiraEn,Revocado,IpOrigen")] ApiToken apiToken)
        {
            if (id != apiToken.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(apiToken);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApiTokenExists(apiToken.Id))
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
            ViewData["ApiClienteId"] = new SelectList(_context.ApiClientes, "Id", "Id", apiToken.ApiClienteId);
            return View(apiToken);
        }

        // GET: ApiToken/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiToken = await _context.ApiTokens
                .Include(a => a.ApiCliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (apiToken == null)
            {
                return NotFound();
            }

            return View(apiToken);
        }

        // POST: ApiToken/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var apiToken = await _context.ApiTokens.FindAsync(id);
            if (apiToken != null)
            {
                _context.ApiTokens.Remove(apiToken);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApiTokenExists(Guid id)
        {
            return _context.ApiTokens.Any(e => e.Id == id);
        }
    }
}
