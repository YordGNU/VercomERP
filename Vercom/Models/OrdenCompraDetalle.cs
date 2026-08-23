namespace Vercom.Models;

public partial class OrdenCompraDetalle
{
    public Guid Id { get; set; }

    public Guid OrdenCompraId { get; set; }

    public Guid ProductoId { get; set; }

    public decimal CantidadSolicitada { get; set; }

    public decimal CantidadRecibida { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal? SubtotalLinea { get; set; }

    public virtual OrdenCompra OrdenCompra { get; set; } = null!;

    public virtual Producto Producto { get; set; } = null!;
}
