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
    public class ApiClienteController : Controller
    {
        private readonly AppDbContext _context;

        public ApiClienteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ApiCliente
        public async Task<IActionResult> Index()
        {
            return View(await _context.ApiClientes.ToListAsync());
        }

        // GET: ApiCliente/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiCliente = await _context.ApiClientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (apiCliente == null)
            {
                return NotFound();
            }

            return View(apiCliente);
        }

        // GET: ApiCliente/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ApiCliente/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntidadId,Nombre,Tipo,ClientId,ClientSecretHash,Scopes,Activo,CreadoPor,CreadoEn,RevocadoEn")] ApiCliente apiCliente)
        {
            if (ModelState.IsValid)
            {
                apiCliente.Id = Guid.NewGuid();
                _context.Add(apiCliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(apiCliente);
        }

        // GET: ApiCliente/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiCliente = await _context.ApiClientes.FindAsync(id);
            if (apiCliente == null)
            {
                return NotFound();
            }
            return View(apiCliente);
        }

        // POST: ApiCliente/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EntidadId,Nombre,Tipo,ClientId,ClientSecretHash,Scopes,Activo,CreadoPor,CreadoEn,RevocadoEn")] ApiCliente apiCliente)
        {
            if (id != apiCliente.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(apiCliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApiClienteExists(apiCliente.Id))
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
            return View(apiCliente);
        }

        // GET: ApiCliente/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiCliente = await _context.ApiClientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (apiCliente == null)
            {
                return NotFound();
            }

            return View(apiCliente);
        }

        // POST: ApiCliente/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var apiCliente = await _context.ApiClientes.FindAsync(id);
            if (apiCliente != null)
            {
                _context.ApiClientes.Remove(apiCliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApiClienteExists(Guid id)
        {
            return _context.ApiClientes.Any(e => e.Id == id);
        }
    }
}
