using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class ClientFormViewModel
{
    public Cliente Client { get; set; } = new();
    public IEnumerable<SelectListItem> ListasPrecio { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> CuentasContables { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Gestión de Cliente";
}

public class ProviderFormViewModel
{
    public Proveedor Provider { get; set; } = new();
    public IEnumerable<SelectListItem> CuentasContables { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Gestión de Proveedor";
}

public class EconomicContractViewModel
{
    public ContratoEconomico Contract { get; set; } = new();
    public IEnumerable<SelectListItem> Clientes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Proveedores { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Contrato Económico (RF-50)";
}

public class PriceLimitFormViewModel
{
    public TopePrecioMfp Limit { get; set; } = new();
    public IEnumerable<SelectListItem> Productos { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Familias { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Tope de Precio (MFP)";
}

public class SalesReturnFormViewModel
{
    public DevolucionVentum Return { get; set; } = new();
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = "Devolución de Venta";
}
