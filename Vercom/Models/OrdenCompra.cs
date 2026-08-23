namespace Vercom.Models;

public partial class OrdenCompra
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public string NumeroOrden { get; set; } = null!;

    public Guid ProveedorId { get; set; }

    public Guid? ContratoId { get; set; }

    public Guid AlmacenDestinoId { get; set; }

    public DateOnly Fecha { get; set; }

    public DateOnly? FechaEntregaEsperada { get; set; }

    public string Moneda { get; set; } = null!;

    public decimal Subtotal { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;

    public Guid? AprobadoPor { get; set; }

    public Guid? CreadoPor { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Almacen AlmacenDestino { get; set; } = null!;

    public virtual Usuario? AprobadoPorNavigation { get; set; }

    public virtual ContratoEconomico? Contrato { get; set; }

    public virtual Usuario? CreadoPorNavigation { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<OrdenCompraDetalle> OrdenCompraDetalles { get; set; } = new List<OrdenCompraDetalle>();

    public virtual Proveedor Proveedor { get; set; } = null!;

    public virtual ICollection<RecepcionCompra> RecepcionCompras { get; set; } = new List<RecepcionCompra>();
}
