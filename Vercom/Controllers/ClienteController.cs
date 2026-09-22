using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class ClienteController : Controller
{
    private readonly ICommercialService _commercialService;

    public ClienteController(ICommercialService commercialService)
    {
        _commercialService = commercialService;
    }

    [Authorize(Policy = "COMERCIAL.CLIENTE.VER")]
    public async Task<IActionResult> Index(string? search, string? segment, string? estado, int page = 1)
    {
        bool? activo = estado switch { "activa" => true, "inactiva" => false, _ => null };
        var vm = await _commercialService.GetClientIndexAsync(search, segment, activo, page);
        return View(vm);
    }

    [Authorize(Policy = "COMERCIAL.CLIENTE.VER")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();
        var cliente = await _commercialService.GetClientByIdAsync(id.Value);
        if (cliente == null) return NotFound();
        return View(cliente);
    }

    [Authorize(Policy = "COMERCIAL.CLIENTE.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _commercialService.GetClientFormContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.CLIENTE.CREAR")]
    public async Task<IActionResult> Create(ClientFormViewModel vm)
    {
        var client = vm.Client;
        ModelState.Remove("Client.Entidad");

        if (ModelState.IsValid)
        {
            var result = await _commercialService.CreateClientAsync(client);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            }
            return Json(new { success = false, message = result.Message });
        }

        var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, message = "Verifique los datos del cliente.", errors = errorList });
    }

    [Authorize(Policy = "COMERCIAL.CLIENTE.EDITAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var client = await _commercialService.GetClientByIdAsync(id.Value);
        if (client == null) return NotFound();

        var vm = await _commercialService.GetClientFormContextAsync(client);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "COMERCIAL.CLIENTE.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, ClientFormViewModel vm)
    {
        var client = vm.Client;
        if (id != client.Id) return NotFound();

        ModelState.Remove("Client.Entidad");

        if (ModelState.IsValid)
        {
            var result = await _commercialService.UpdateClientAsync(client);
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
