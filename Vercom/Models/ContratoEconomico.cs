namespace Vercom.Models;

public partial class ContratoEconomico
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public string TerceroTipo { get; set; } = null!;

    public Guid? ClienteId { get; set; }

    public Guid? ProveedorId { get; set; }

    public string NumeroContrato { get; set; } = null!;

    public string Objeto { get; set; } = null!;

    public DateOnly FechaFirma { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public DateOnly? FechaFinOriginal { get; set; }

    public decimal? MontoTotal { get; set; }

    public string? DocumentoUrl { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Cliente? Cliente { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<ContratoEconomicoSuplemento> Suplementos { get; set; } = new List<ContratoEconomicoSuplemento>();

    public virtual ICollection<FacturaVentum> FacturaVenta { get; set; } = new List<FacturaVentum>();

    public virtual ICollection<OrdenCompra> OrdenCompras { get; set; } = new List<OrdenCompra>();

    public virtual Proveedor? Proveedor { get; set; }
}
