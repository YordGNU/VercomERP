using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Vercom.Models;

[ModelMetadataType(typeof(OrdenCompraMetadata))]
public partial class OrdenCompra
{
}

public class OrdenCompraMetadata
{
    [Required(ErrorMessage = "Debe seleccionar un proveedor")]
    [Display(Name = "Proveedor")]
    public Guid ProveedorId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un almacén de destino")]
    [Display(Name = "Almacén de Destino")]
    public Guid AlmacenDestinoId { get; set; }

    [Required(ErrorMessage = "La fecha de la orden es obligatoria")]
    [Display(Name = "Fecha de Emisión")]
    public DateOnly Fecha { get; set; }

    [Display(Name = "Fecha de Entrega Esperada")]
    public DateOnly? FechaEntregaEsperada { get; set; }

    [Range(0.01, 999999999999.99, ErrorMessage = "El total de la orden debe ser mayor a cero")]
    [Display(Name = "Total Estimado")]
    public decimal Total { get; set; }
}
