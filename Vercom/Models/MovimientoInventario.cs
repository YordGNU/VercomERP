namespace Vercom.Models;

public partial class MovimientoInventario
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public int TipoMovimientoId { get; set; }

    public string NumeroDocumento { get; set; } = null!;

    public Guid? AlmacenOrigenId { get; set; }

    public Guid? AlmacenDestinoId { get; set; }

    public DateTimeOffset Fecha { get; set; }

    public string? ReferenciaExternaTipo { get; set; }

    public Guid? ReferenciaExternaId { get; set; }

    public string Canal { get; set; } = null!;

    public Guid? DispositivoPosId { get; set; }

    public string? Observaciones { get; set; }

    public Guid? AsientoId { get; set; }

    public Guid? CreadoPor { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Almacen? AlmacenDestino { get; set; }

    public virtual Almacen? AlmacenOrigen { get; set; }

    public virtual AsientoContable? Asiento { get; set; }

    public virtual ICollection<ConteoFisicoDetalle> ConteoFisicoDetalles { get; set; } = new List<ConteoFisicoDetalle>();

    public virtual Usuario? CreadoPorNavigation { get; set; }

    public virtual ICollection<DevolucionVentum> DevolucionVenta { get; set; } = new List<DevolucionVentum>();

    public virtual DispositivoPo? DispositivoPos { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<FacturaVentaDetalle> FacturaVentaDetalles { get; set; } = new List<FacturaVentaDetalle>();

    public virtual ICollection<MovimientoInventarioDetalle> MovimientoInventarioDetalles { get; set; } = new List<MovimientoInventarioDetalle>();

    public virtual ICollection<OrdenProduccionConsumo> OrdenProduccionConsumos { get; set; } = new List<OrdenProduccionConsumo>();

    public virtual ICollection<RecepcionCompra> RecepcionCompras { get; set; } = new List<RecepcionCompra>();

    public virtual TipoMovimiento TipoMovimiento { get; set; } = null!;
}
