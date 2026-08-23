namespace Vercom.Models;

public partial class CuentaPorPagar
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid ProveedorId { get; set; }

    public string DocumentoOrigenTipo { get; set; } = null!;

    public Guid DocumentoOrigenId { get; set; }

    public Guid? AsientoOrigenId { get; set; }

    public DateOnly FechaEmision { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public decimal MontoOriginal { get; set; }

    public decimal SaldoPendiente { get; set; }

    public string Moneda { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public DateTimeOffset CreadoEn { get; set; }

    public virtual AsientoContable? AsientoOrigen { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<PagoAplicado> PagoAplicados { get; set; } = new List<PagoAplicado>();

    public virtual Proveedor Proveedor { get; set; } = null!;

    public virtual ICollection<RecepcionCompra> RecepcionCompras { get; set; } = new List<RecepcionCompra>();
}
