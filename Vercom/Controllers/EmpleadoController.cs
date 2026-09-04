using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;
using Vercom.Services;
using Vercom.ViewModels;

namespace Vercom.Controllers;

[Authorize]
public class EmpleadoController : Controller
{
    private readonly IHRService _hrService;
    private readonly IHRReportService _hrReportService;
    private readonly IAdminService _adminService;

    public EmpleadoController(IHRService hrService, IHRReportService hrReportService, IAdminService adminService)
    {
        _hrService = hrService;
        _hrReportService = hrReportService;
        _adminService = adminService;
    }

    [Authorize(Policy = "RRHH.EMPLEADO.VER")]
    public async Task<IActionResult> Index(string? search, Guid? cargoId, Guid? sucursalId, string? estado, DateOnly? desde, DateOnly? hasta)
    {
        var empleados = await _hrService.GetEmployeesAsync(search, cargoId, sucursalId, estado, desde, hasta);
        ViewBag.Stats = await _hrReportService.GetGeneralStatsAsync(Guid.Empty);

        var cargos = await _hrService.GetCargosAsync();
        ViewBag.CargoId = new SelectList(cargos.Select(c => new { Id = c.Id, DisplayText = $"{c.Codigo} - {c.Nombre}" }), "Id", "DisplayText");

        var sucursales = await _adminService.GetSucursalesAsync();
        ViewBag.SucursalId = new SelectList(sucursales.Select(c => new { Id = c.Id, DisplayText = $"{c.Codigo} - {c.Nombre}" }), "Id", "DisplayText");

        ViewBag.CurrentSearch = search;
        ViewBag.CurrentCargo = cargoId;
        ViewBag.CurrentSucursal = sucursalId;
        ViewBag.CurrentEstado = estado;
        ViewBag.CurrentDesde = desde?.ToString("yyyy-MM-dd");
        ViewBag.CurrentHasta = hasta?.ToString("yyyy-MM-dd");

        return View(empleados);
    }

    [Authorize(Policy = "RRHH.EMPLEADO.VER")]
    public async Task<IActionResult> File(Guid? id)
    {
        if (id == null) return NotFound();

        try
        {
            // Ahora obtenemos el ViewModel completo desde el servicio
            var viewModel = await _hrService.GetExpedienteAsync(id.Value);
            return View(viewModel);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize(Policy = "RRHH.EMPLEADO.CREAR")]
    public async Task<IActionResult> Create()
    {
        var vm = await _hrService.GetEmployeeCreateContextAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.EMPLEADO.CREAR")]
    public async Task<IActionResult> Create(EmployeeCreateViewModel vm)
    {
        var empleado = vm.Empleado;

        // Limpiar validaciones de objetos de navegación
        ModelState.Remove("Empleado.Cargo");
        ModelState.Remove("Empleado.Entidad");
        ModelState.Remove("Empleado.Sucursal");

        // Ignorar campos que se asignan en el servidor
        ModelState.Remove("Empleado.Id");
        ModelState.Remove("Empleado.EntidadId");
        ModelState.Remove("Empleado.CreadoEn");

        if (ModelState.IsValid)
        {
            var result = await _hrService.CreateEmployeeAsync(empleado);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }
        else
        {
            var errors = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            ModelState.AddModelError("", $"Verifique los datos: {errors}");
        }

        // Recargar listas para la vista en caso de error
        var contextVm = await _hrService.GetEmployeeCreateContextAsync(empleado);
        return View(contextVm);
    }

    [Authorize(Policy = "RRHH.EMPLEADO.EDITAR")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        var empleado = await _hrService.GetEmployeeByIdAsync(id.Value);
        if (empleado == null) return NotFound();

        var vm = await _hrService.GetEmployeeCreateContextAsync(empleado);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.EMPLEADO.EDITAR")]
    public async Task<IActionResult> Edit(Guid id, EmployeeCreateViewModel vm)
    {
        var empleado = vm.Empleado;
        if (id != empleado.Id) return NotFound();

        ModelState.Remove("Empleado.Cargo");
        ModelState.Remove("Empleado.Entidad");
        ModelState.Remove("Empleado.Sucursal");
        ModelState.Remove("Empleado.EntidadId");

        if (ModelState.IsValid)
        {
            var result = await _hrService.UpdateEmployeeAsync(empleado);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(File), new { id = empleado.Id });
            }
            ModelState.AddModelError("", result.Message);
        }

        var contextVm = await _hrService.GetEmployeeCreateContextAsync(empleado);
        return View(contextVm);
    }

    // GESTIÓN DE CONTRATOS
    [Authorize(Policy = "RRHH.EMPLEADO.EDITAR")]
    public async Task<IActionResult> AddContract(Guid id)
    {
        var vm = await _hrService.GetContractCreateContextAsync(id);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.EMPLEADO.EDITAR")]
    public async Task<IActionResult> AddContract(EmployeeContractViewModel vm)
    {
        var contrato = vm.Contrato;

        // 1. Limpiar validaciones de campos de visualización y navegación
        ModelState.Remove("NombreEmpleado");
        ModelState.Remove("Cargos");
        ModelState.Remove("TiposContrato");
        ModelState.Remove("Contrato.Empleado");
        ModelState.Remove("Contrato.Cargo");
        ModelState.Remove("Contrato.Id");
        ModelState.Remove("Contrato.CreadoEn");
        ModelState.Remove("Contrato.DocumentoUrl");
        ModelState.Remove("Documento");

        if (string.IsNullOrEmpty(contrato.Estado)) contrato.Estado = "VIGENTE";

        if (ModelState.IsValid)
        {
            var result = await _hrService.AddContractAsync(contrato, vm.Documento);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(File), new { id = contrato.EmpleadoId });
            }
            ModelState.AddModelError("", result.Message);
        }
        else
        {
            var errors = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            ModelState.AddModelError("", "Verifique los datos: " + errors);
        }

        // Recargar contexto de visualización
        var contextVm = await _hrService.GetContractCreateContextAsync(contrato.EmpleadoId);
        contextVm.Contrato = contrato;
        return View(contextVm);
    }

    // GESTIÓN DE CERTIFICADOS MÉDICOS
    [Authorize(Policy = "RRHH.EMPLEADO.EDITAR")]
    public async Task<IActionResult> AddMedicalCertificate(Guid id)
    {
        var vm = await _hrService.GetMedicalCertificateCreateContextAsync(id);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.EMPLEADO.EDITAR")]
    public async Task<IActionResult> AddMedicalCertificate(CertificadoMedico certificado)
    {
        if (ModelState.IsValid)
        {
            var result = await _hrService.AddMedicalCertificateAsync(certificado);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(File), new { id = certificado.EmpleadoId });
            }
            ModelState.AddModelError("", result.Message);
        }
        var vm = await _hrService.GetMedicalCertificateCreateContextAsync(certificado.EmpleadoId);
        return View(vm);
    }

    // BAJA LABORAL (RF-21)
    [Authorize(Policy = "RRHH.EMPLEADO.ELIMINAR")]
    public async Task<IActionResult> Terminate(Guid id)
    {
        var vm = await _hrService.GetTerminationContextAsync(id);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RRHH.EMPLEADO.ELIMINAR")]
    public async Task<IActionResult> Terminate(Guid EmpleadoId, DateOnly FechaBaja, string MotivoBaja)
    {
        var result = await _hrService.TerminateEmployeeAsync(EmpleadoId, FechaBaja, MotivoBaja);
        if (result.Succeeded)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Terminate), new { id = EmpleadoId });
    }

    // REPORTE OFICIAL SC-4-08 (RF-25)
    [Authorize(Policy = "RRHH.EMPLEADO.VER")]
    public async Task<IActionResult> SC408(Guid id)
    {
        var employee = await _hrService.GetEmployeeByIdAsync(id);
        if (employee == null) return NotFound();

        var reportData = await _hrReportService.GetSC408ReportAsync(id);
        ViewBag.Employee = employee;
        return View(reportData);
    }
}
