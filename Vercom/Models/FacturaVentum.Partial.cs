using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Vercom.Models;

[ModelMetadataType(typeof(FacturaVentumMetadata))]
public partial class FacturaVentum
{
}

public class FacturaVentumMetadata
{
    [Required(ErrorMessage = "Debe seleccionar un cliente")]
    [Display(Name = "Cliente")]
    public Guid ClienteId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un almacén para el despacho")]
    [Display(Name = "Almacén/Punto de Venta")]
    public Guid AlmacenId { get; set; }

    [Required(ErrorMessage = "La serie es obligatoria")]
    [StringLength(10)]
    [Display(Name = "Serie")]
    public string Serie { get; set; } = null!;

    [Range(0.01, 999999999999.99, ErrorMessage = "El total de la factura no puede ser cero")]
    [Display(Name = "Total Factura")]
    public decimal Total { get; set; }

    [Required]
    [Display(Name = "Tipo de Venta")]
    public string TipoVenta { get; set; } = null!;
}
