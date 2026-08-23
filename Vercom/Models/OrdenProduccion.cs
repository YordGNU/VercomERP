namespace Vercom.Models;

public partial class OrdenProduccion
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public string NumeroOrden { get; set; } = null!;

    public Guid ProductoTerminadoId { get; set; }

    public Guid ListaMaterialesId { get; set; }

    public Guid? FichaCostoId { get; set; }

    public Guid AlmacenInsumosId { get; set; }

    public Guid AlmacenProductoId { get; set; }

    public decimal CantidadPlanificada { get; set; }

    public decimal CantidadProducida { get; set; }

    public DateOnly? FechaInicioPlan { get; set; }

    public DateOnly? FechaFinPlan { get; set; }

    public DateTimeOffset? FechaInicioReal { get; set; }

    public DateTimeOffset? FechaFinReal { get; set; }

    public string Estado { get; set; } = null!;

    public decimal? CostoRealTotal { get; set; }

    public Guid? AsientoConsumoId { get; set; }

    public Guid? AsientoTerminadoId { get; set; }

    public Guid? CreadoPor { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Almacen AlmacenInsumos { get; set; } = null!;

    public virtual Almacen AlmacenProducto { get; set; } = null!;

    public virtual ICollection<AnalisisDesviacion> AnalisisDesviacions { get; set; } = new List<AnalisisDesviacion>();

    public virtual AsientoContable? AsientoConsumo { get; set; }

    public virtual AsientoContable? AsientoTerminado { get; set; }

    public virtual Usuario? CreadoPorNavigation { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual FichaCosto? FichaCosto { get; set; }

    public virtual ListaMateriale ListaMateriales { get; set; } = null!;

    public virtual ICollection<Merma> Mermas { get; set; } = new List<Merma>();

    public virtual ICollection<OrdenProduccionConsumo> OrdenProduccionConsumos { get; set; } = new List<OrdenProduccionConsumo>();

    public virtual Producto ProductoTerminado { get; set; } = null!;
}
