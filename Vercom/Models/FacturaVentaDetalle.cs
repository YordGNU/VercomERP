namespace Vercom.Models;

public partial class FacturaVentaDetalle
{
    public Guid Id { get; set; }

    public Guid FacturaId { get; set; }

    public Guid ProductoId { get; set; }

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal DescuentoPorcentaje { get; set; }

    public decimal? CostoUnitarioVenta { get; set; }

    public decimal ImpuestoPorcentaje { get; set; }

    public decimal SubtotalLinea { get; set; }

    public Guid? MovimientoInventarioId { get; set; }

    public virtual ICollection<DevolucionVentaDetalle> DevolucionVentaDetalles { get; set; } = new List<DevolucionVentaDetalle>();

    public virtual FacturaVentum Factura { get; set; } = null!;

    public virtual MovimientoInventario? MovimientoInventario { get; set; }

    public virtual Producto Producto { get; set; } = null!;
}
