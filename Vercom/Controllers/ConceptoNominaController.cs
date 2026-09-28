using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize]
public class ConceptoNominaController : Controller
{
    private readonly IPayrollService _payrollService;

    public ConceptoNominaController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [Authorize(Policy = "RRHH.NOMINA.VER")]
    public async Task<IActionResult> Index()
    {
        var items = await _payrollService.GetConceptsAsync();
        return View(items);
    }

    [Authorize(Policy = "RRHH.NOMINA.CREAR")]
    public IActionResult Create() => View(new ConceptoNomina { Activo = true, Tipo = "DEVENGO" });

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.NOMINA.CREAR")]
    public async Task<IActionResult> Create(ConceptoNomina concept)
    {
        ModelState.Remove("Entidad");
        if (ModelState.IsValid)
        {
            var result = await _payrollService.CreateConceptAsync(concept);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        return View(concept);
    }
}
