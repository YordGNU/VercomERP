namespace Vercom.Models;

public partial class MovimientoInventarioDetalle
{
    public Guid Id { get; set; }

    public Guid MovimientoId { get; set; }

    public Guid ProductoId { get; set; }

    public decimal Cantidad { get; set; }

    public decimal? CostoUnitario { get; set; }

    public string? Lote { get; set; }

    public DateOnly? FechaVencimiento { get; set; }

    public string? Observaciones { get; set; }

    public virtual MovimientoInventario Movimiento { get; set; } = null!;

    public virtual Producto Producto { get; set; } = null!;
}
