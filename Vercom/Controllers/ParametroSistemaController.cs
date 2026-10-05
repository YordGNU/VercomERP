using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize(Policy = "SEGURIDAD.ROL.ASIGNAR")]
public class ParametroSistemaController : Controller
{
    private readonly IAdminService _adminService;

    public ParametroSistemaController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _adminService.GetParametersAsync();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _adminService.GetParameterFormContextAsync(id);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ParametroFormViewModel vm)
    {
        var entry = vm.Parametro;
        if (id == 0) return Json(new { success = false, message = "El identificador del parámetro no es válido." });

        ModelState.Remove("Parametro.Entidad");

        if (ModelState.IsValid)
        {
            var result = await _adminService.SaveParameterAsync(entry);
            if (result.Succeeded)
            {
                return Json(new
                {
                    success = true,
                    message = result.Message ?? "Parámetro actualizado correctamente.",
                    redirectUrl = Url.Action(nameof(Index))
                });
            }

            return Json(new { success = false, message = result.Message ?? "No se pudo guardar el parámetro." });
        }

        return Json(new
        {
            success = false,
            message = "Revise los campos señalados.",
            errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray()
        });
    }
}
