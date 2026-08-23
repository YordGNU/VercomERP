namespace Vercom.Models;

public partial class TopePrecioMfp
{
    public Guid Id { get; set; }

    public Guid? ProductoId { get; set; }

    public Guid? FamiliaId { get; set; }

    public decimal PrecioMaximo { get; set; }

    public DateOnly VigenteDesde { get; set; }

    public DateOnly? VigenteHasta { get; set; }

    public string? ResolucionReferencia { get; set; }

    public virtual FamiliaProducto? Familia { get; set; }

    public virtual Producto? Producto { get; set; }
}
