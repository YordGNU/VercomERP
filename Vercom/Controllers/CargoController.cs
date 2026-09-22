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


    [HttpGet]
    [Authorize(Policy = "RRHH.CARGO.VER")]
    public async Task<IActionResult> GetScale(Guid id)
    {
        var cargo = await _hrService.GetCargoByIdAsync(id);
        if (cargo == null) return NotFound();
        return Json(new { min = cargo.SalarioEscalaMin, max = cargo.SalarioEscalaMax });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.CARGO.CREAR")]
    public async Task<IActionResult> Create(Cargo cargo)
    {
        if (ModelState.IsValid)
        {
            var result = await _hrService.CreateCargoAsync(cargo);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
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
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.CARGO.ELIMINAR")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _hrService.DeleteCargoAsync(id);
        return Json(new { success = result.Succeeded, message = result.Message });
    }
}
