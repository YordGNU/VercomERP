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

    [Authorize(Policy = "COMERCIAL.CONTRATO_SUPLEMENTO.VER")]
    public async Task<IActionResult> SuplementoCreate(Guid? id)
    {
        if (id == null) return NotFound();

        var contract = await _commercialService.GetContractByIdAsync(id.Value);
        if (contract == null) return NotFound();

        var vm = await _commercialService.GetContractSuplementoFormContextAsync(id.Value);
        if (vm.ContratoId == Guid.Empty) return NotFound();

        vm.NumeroContrato = contract.NumeroContrato;
        vm.TerceroTipo = contract.TerceroTipo;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.CONTRATO_SUPLEMENTO.CREAR")]
    public async Task<IActionResult> SuplementoCreate(Guid id, ContractSuplementoFormViewModel vm)
    {
        var suplemento = vm.Suplemento;
        if (id != suplemento.ContratoId) return NotFound();

        ModelState.Remove("Suplemento.Contrato");
        ModelState.Remove("Suplemento.Entidad");
        ModelState.Remove("Suplemento.EntidadId");
        ModelState.Remove("Suplemento.Estado");
        ModelState.Remove("Suplemento.NumeroSuplemento");
        ModelState.Remove("Suplemento.CreadoEn");
        ModelState.Remove("Suplemento.CreadoPor");
        ModelState.Remove("Suplemento.Id");
        ModelState.Remove("Suplemento.DocumentoUrl");
        ModelState.Remove("Suplemento.MotivoAnulacion");

        if (ModelState.IsValid)
        {
            var result = await _commercialService.CreateContractSuplementoAsync(suplemento, vm.Documento);
            if (result.Succeeded)
            {
                return Json(new
                {
                    success = true,
                    message = result.Message,
                    redirectUrl = Url.Action(nameof(Details), new { id })
                });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Errores de validación.", errors = errorList });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.CONTRATO_SUPLEMENTO.ANULAR")]
    public async Task<IActionResult> SuplementoAnular(Guid id, Guid suplementoId, string? motivo)
    {
        var supplement = await _commercialService.GetContractSuplementosAsync(id);
        if (supplement.All(s => s.Id != suplementoId)) return NotFound();

        if (string.IsNullOrWhiteSpace(motivo))
        {
            return Json(new { success = false, message = "Indique el motivo de la anulación." });
        }

        var result = await _commercialService.AnularContractSuplementoAsync(suplementoId, motivo);
        if (result.Succeeded)
        {
            return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Details), new { id }) });
        }
        return Json(new { success = false, message = result.Message });
    }
}
