namespace Vercom.Models;

public partial class Merma
{
    public Guid Id { get; set; }

    public Guid? OrdenProduccionId { get; set; }

    public Guid ProductoId { get; set; }

    public decimal Cantidad { get; set; }

    public string Causa { get; set; } = null!;

    public decimal? ValorContable { get; set; }

    public Guid? AsientoId { get; set; }

    public DateOnly Fecha { get; set; }

    public string? Observaciones { get; set; }

    public virtual AsientoContable? Asiento { get; set; }

    public virtual OrdenProduccion? OrdenProduccion { get; set; }

    public virtual Producto Producto { get; set; } = null!;
}
