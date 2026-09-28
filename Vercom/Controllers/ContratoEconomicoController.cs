using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Security;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class ContratoEconomicoController : Controller
{
    private readonly ICommercialService _commercialService;
    private readonly IEntidadProvider _entidadProvider;

    public ContratoEconomicoController(ICommercialService commercialService, IEntidadProvider entidadProvider)
    {
        _commercialService = commercialService;
        _entidadProvider = entidadProvider;
    }

    [Authorize(Policy = "COMERCIAL.CONTRATO.VER")]
    public async Task<IActionResult> Index(string? search, string? type, string? status)
    {
        var contracts = await _commercialService.GetContractsAsync(search, type, status);
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentType = type;
        ViewBag.CurrentStatus = status;
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
        var document = vm.DocumentoContrato;
        var fecha = DateOnly.FromDateTime(DateTime.Now);
        if (contract.FechaFin < fecha) contract.Estado = "VENCIDO";

        ModelState.Remove("Contract.Entidad");
        ModelState.Remove("Contract.Cliente");
        ModelState.Remove("Contract.Proveedor");
        ModelState.Remove("Contract.EntidadId");

        if (ModelState.IsValid)
        {
            contract.EntidadId = _entidadProvider.CurrentEntidadId;
            var result = await _commercialService.CreateContractAsync(contract, document);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }

    [Authorize(Policy = "COMERCIAL.CONTRATO.EDITAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var contract = await _commercialService.GetContractByIdAsync(id.Value);
        if (contract == null) return NotFound();

        var vm = await _commercialService.GetContractFormContextAsync(contract);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.CONTRATO.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, EconomicContractViewModel vm)
    {
        var contract = vm.Contract;
        if (id != contract.Id) return NotFound();

        ModelState.Remove("Contract.Entidad");
        ModelState.Remove("Contract.Cliente");
        ModelState.Remove("Contract.Proveedor");
        ModelState.Remove("Contract.EntidadId");
        ModelState.Remove("Contract.Estado");

        if (ModelState.IsValid)
        {
            contract.EntidadId = _entidadProvider.CurrentEntidadId;
            var result = await _commercialService.UpdateContractAsync(contract, vm.DocumentoContrato, vm.QuitarDocumento);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }
}
