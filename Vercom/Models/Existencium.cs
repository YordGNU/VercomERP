namespace Vercom.Models;

public partial class Existencium
{
    public Guid Id { get; set; }

    public Guid AlmacenId { get; set; }

    public Guid ProductoId { get; set; }

    public decimal Cantidad { get; set; }

    public decimal CostoPromedio { get; set; }

    public decimal StockMinimo { get; set; }

    public decimal? StockMaximo { get; set; }

    public DateTimeOffset ActualizadoEn { get; set; }

    public virtual Almacen Almacen { get; set; } = null!;

    public virtual Producto Producto { get; set; } = null!;
}
