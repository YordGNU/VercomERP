namespace Vercom.Models;

public partial class Producto
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public string Codigo { get; set; } = null!;

    public string? CodigoBarras { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public Guid? FamiliaId { get; set; }

    public int UnidadMedidaId { get; set; }

    public string Tipo { get; set; } = null!;

    public Guid? CuentaInventarioId { get; set; }

    public Guid? CuentaCostoVentaId { get; set; }

    public Guid? CuentaIngresoId { get; set; }

    public decimal? PrecioVentaActual { get; set; }

    public bool AplicaImpuestoVentas { get; set; }

    public bool Activo { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public DateTimeOffset ActualizadoEn { get; set; }

    public virtual ICollection<ConteoFisicoDetalle> ConteoFisicoDetalles { get; set; } = new List<ConteoFisicoDetalle>();

    public virtual CuentaContable? CuentaCostoVenta { get; set; }

    public virtual CuentaContable? CuentaIngreso { get; set; }

    public virtual CuentaContable? CuentaInventario { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<Existencium> Existencia { get; set; } = new List<Existencium>();

    public virtual ICollection<FacturaVentaDetalle> FacturaVentaDetalles { get; set; } = new List<FacturaVentaDetalle>();

    public virtual FamiliaProducto? Familia { get; set; }

    public virtual ICollection<FichaCosto> FichaCostos { get; set; } = new List<FichaCosto>();

    public virtual ICollection<ListaMateriale> ListaMateriales { get; set; } = new List<ListaMateriale>();

    public virtual ICollection<ListaMaterialesDetalle> ListaMaterialesDetalles { get; set; } = new List<ListaMaterialesDetalle>();

    public virtual ICollection<ListaPrecioDetalle> ListaPrecioDetalles { get; set; } = new List<ListaPrecioDetalle>();

    public virtual ICollection<Merma> Mermas { get; set; } = new List<Merma>();

    public virtual ICollection<MovimientoInventarioDetalle> MovimientoInventarioDetalles { get; set; } = new List<MovimientoInventarioDetalle>();

    public virtual ICollection<OrdenCompraDetalle> OrdenCompraDetalles { get; set; } = new List<OrdenCompraDetalle>();

    public virtual ICollection<OrdenProduccionConsumo> OrdenProduccionConsumos { get; set; } = new List<OrdenProduccionConsumo>();

    public virtual ICollection<OrdenProduccion> OrdenProduccions { get; set; } = new List<OrdenProduccion>();

    public virtual ICollection<PlanProduccionDetalle> PlanProduccionDetalles { get; set; } = new List<PlanProduccionDetalle>();

    public virtual ICollection<TopePrecioMfp> TopePrecioMfps { get; set; } = new List<TopePrecioMfp>();

    public virtual UnidadMedidum UnidadMedida { get; set; } = null!;
}
