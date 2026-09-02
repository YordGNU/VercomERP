using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class CargoController : Controller
{
    private readonly IHRService _hrService;

    public CargoController(IHRService hrService)
    {
        _hrService = hrService;
    }

    [Authorize(Policy = "RRHH.CARGO.VER")]
    public async Task<IActionResult> Index()
    {
        var cargos = await _hrService.GetCargosAsync();
        return View(cargos);
    }

    [Authorize(Policy = "RRHH.CARGO.CREAR")]
    public IActionResult Create() => View(new Cargo());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.CARGO.CREAR")]
    public async Task<IActionResult> Create(Cargo cargo)
    {

        var result = await _hrService.CreateCargoAsync(cargo);
        if (result.Succeeded) return RedirectToAction(nameof(Index));
        ModelState.AddModelError("", result.Message);
        return View(cargo);
    }

    [Authorize(Policy = "RRHH.CARGO.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();
        var cargo = await _hrService.GetCargoByIdAsync(id.Value);
        if (cargo == null) return NotFound();
        return View(cargo);
    }

    [Authorize(Policy = "RRHH.CARGO.EDITAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var cargo = await _hrService.GetCargoByIdAsync(id.Value);
        if (cargo == null) return NotFound();
        return View(cargo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.CARGO.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, Cargo cargo)
    {
        if (id != cargo.Id) return NotFound();
        if (ModelState.IsValid)
        {
            var result = await _hrService.UpdateCargoAsync(cargo);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        return View(cargo);
    }
}
