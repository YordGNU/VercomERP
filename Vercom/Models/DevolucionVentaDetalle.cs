namespace Vercom.Models;

public partial class DevolucionVentaDetalle
{
    public Guid Id { get; set; }

    public Guid DevolucionId { get; set; }

    public Guid FacturaDetalleId { get; set; }

    public decimal CantidadDevuelta { get; set; }

    public virtual DevolucionVentum Devolucion { get; set; } = null!;

    public virtual FacturaVentaDetalle FacturaDetalle { get; set; } = null!;
}
