namespace Vercom.Models;

public partial class DevolucionVentum
{
    public Guid Id { get; set; }

    public Guid FacturaId { get; set; }

    public DateOnly Fecha { get; set; }

    public string Motivo { get; set; } = null!;

    public decimal TotalDevuelto { get; set; }

    public Guid? MovimientoInventarioId { get; set; }

    public Guid? AsientoId { get; set; }

    public Guid? AutorizadoPor { get; set; }

    public virtual AsientoContable? Asiento { get; set; }

    public virtual Usuario? AutorizadoPorNavigation { get; set; }

    public virtual ICollection<DevolucionVentaDetalle> DevolucionVentaDetalles { get; set; } = new List<DevolucionVentaDetalle>();

    public virtual FacturaVentum Factura { get; set; } = null!;

    public virtual MovimientoInventario? MovimientoInventario { get; set; }
}
