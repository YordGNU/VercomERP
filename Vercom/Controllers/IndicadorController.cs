using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VERCOM.Models;

namespace VERCOM.Controllers;

[Authorize]
public class IndicadorController : Controller
{
    private readonly AppDbContext _context;

    public IndicadorController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _context.Indicadors.OrderBy(x => x.Codigo).ToListAsync();
        return View(items);
    }

    [Authorize(Policy = "REPORTES.INDICADOR.VER")]
    public IActionResult Create()
    {
        return View(new Indicador());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "REPORTES.INDICADOR.VER")]
    public async Task<IActionResult> Create(Indicador model)
    {
        if (ModelState.IsValid)
        {
            _context.Indicadors.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Indicador creado.";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    [Authorize(Policy = "REPORTES.INDICADOR.VER")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Indicadors.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "REPORTES.INDICADOR.VER")]
    public async Task<IActionResult> Edit(int id, Indicador model)
    {
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Indicador actualizado.";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "REPORTES.INDICADOR.VER")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.Indicadors.FindAsync(id);
        if (item == null)
        {
            TempData["Error"] = "No encontrado.";
            return RedirectToAction(nameof(Index));
        }

        _context.Indicadors.Remove(item);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Indicador eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
