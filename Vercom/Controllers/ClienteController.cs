using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Models;
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
    public async Task<IActionResult> Index()
    {
        var clientes = await _commercialService.GetClientsAsync();
        return View(clientes);
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
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _commercialService.GetClientFormContextAsync(client);
        return View(contextVm);
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
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", result.Message);
        }
        var contextVm = await _commercialService.GetClientFormContextAsync(client);
        return View(contextVm);
    }
}
