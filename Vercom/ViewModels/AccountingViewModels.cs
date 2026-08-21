using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class CajaFormViewModel
{
    public Caja Caja { get; set; } = new();
    public IEnumerable<SelectListItem> CuentasContables { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Gestión de Caja";
}

public class BankAccountFormViewModel
{
    public CuentaBancarium BankAccount { get; set; } = new();
    public IEnumerable<SelectListItem> CuentasContables { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Gestión de Cuenta Bancaria";
}

public class CentroCostoFormViewModel
{
    public CentroCosto CentroCosto { get; set; } = new();
    public IEnumerable<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Gestión de Centro de Costo";
}
