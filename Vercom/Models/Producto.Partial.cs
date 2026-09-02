using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Vercom.Models;

[ModelMetadataType(typeof(ProductoMetadata))]
public partial class Producto
{
    // Esta clase extiende la funcionalidad de la clase generada por EF sin tocar el código automático
}

public class ProductoMetadata
{
    [Required(ErrorMessage = "El código SKU es obligatorio")]
    [StringLength(30, ErrorMessage = "El código no puede exceder los 30 caracteres")]
    [Display(Name = "Código (SKU)")]
    public string Codigo { get; set; } = null!;

    [Required(ErrorMessage = "El nombre del producto es obligatorio")]
    [StringLength(200, ErrorMessage = "El nombre es demasiado largo")]
    [Display(Name = "Nombre Comercial")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "Debe seleccionar una unidad de medida")]
    [Display(Name = "Unidad de Medida")]
    public int UnidadMedidaId { get; set; }

    [Required(ErrorMessage = "El precio de venta es obligatorio")]
    [Range(0.01, 999999999.99, ErrorMessage = "El precio debe ser un valor positivo")]
    [Display(Name = "Precio de Venta")]
    public decimal? PrecioVentaActual { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; }
}
