using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class ContratoEconomicoController : Controller
{
    private readonly ICommercialService _commercialService;

    public ContratoEconomicoController(ICommercialService commercialService)
    {
        _commercialService = commercialService;
    }

    [Authorize(Policy = "COMERCIAL.CONTRATO.VER")]
    public async Task<IActionResult> Index()
    {
        var contracts = await _commercialService.GetContractsAsync();
        return View(contracts);
    }

    [Authorize(Policy = "COMERCIAL.CONTRATO.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();
        var contract = await _commercialService.GetContractByIdAsync(id.Value);
        if (contract == null) return NotFound();
        return View(contract);
    }

    [Authorize(Policy = "COMERCIAL.CONTRATO.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _commercialService.GetContractFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.CONTRATO.CREAR")]
    public async Task<IActionResult> Create(EconomicContractViewModel vm)
    {
        var contract = vm.Contract;
        ModelState.Remove("Contract.Entidad");

        if (ModelState.IsValid)
        {
            var result = await _commercialService.CreateContractAsync(contract);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _commercialService.GetContractFormContextAsync(contract);
        return View(contextVm);
    }
}
