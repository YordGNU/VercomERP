using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VERCOM.Models;

namespace VERCOM.Controllers;

[Authorize]
public class TipoObligacionFiscalController : Controller
{
    private readonly AppDbContext _context;

    public TipoObligacionFiscalController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _context.TipoObligacionFiscals
            .OrderBy(x => x.Codigo)
            .ToListAsync();
        return View(items);
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.CREAR")]
    public IActionResult Create()
    {
        return View(new TipoObligacionFiscal());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CUENTA.CREAR")]
    public async Task<IActionResult> Create(TipoObligacionFiscal model)
    {
        if (ModelState.IsValid)
        {
            _context.TipoObligacionFiscals.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Tipo de obligación creado.";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    [Authorize(Policy = "CONTABILIDAD.CUENTA.EDITAR")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.TipoObligacionFiscals.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CUENTA.EDITAR")]
    public async Task<IActionResult> Edit(int id, TipoObligacionFiscal model)
    {
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Tipo de obligación actualizado.";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CONTABILIDAD.CUENTA.ELIMINAR")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.TipoObligacionFiscals.FindAsync(id);
        if (item == null)
        {
            TempData["Error"] = "No encontrado.";
            return RedirectToAction(nameof(Index));
        }

        _context.TipoObligacionFiscals.Remove(item);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Tipo de obligación eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
