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

public class ProviderListViewModel
{
    public IEnumerable<Proveedor> Items { get; set; } = new List<Proveedor>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public string? Search { get; set; }
    public string? TipoPersona { get; set; }
    public bool? Activo { get; set; }

    public int Total { get; set; }
    public int Activos { get; set; }
    public int Inactivos { get; set; }
    public int Juridicas { get; set; }
    public int Naturales { get; set; }

    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

public class ClientListViewModel
{
    public IEnumerable<Cliente> Items { get; set; } = new List<Cliente>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public string? Search { get; set; }
    public string? Segmento { get; set; }
    public bool? Activo { get; set; }

    public int Total { get; set; }
    public int Activos { get; set; }
    public int Inactivos { get; set; }
    public int Minoristas { get; set; }
    public int Mayoristas { get; set; }

    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

public class EconomicContractViewModel
{
    public ContratoEconomico Contract { get; set; } = new();
    public IEnumerable<SelectListItem> Clientes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Proveedores { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Contrato Económico";
    public IFormFile? DocumentoContrato { get; set; }
    public bool QuitarDocumento { get; set; }
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
