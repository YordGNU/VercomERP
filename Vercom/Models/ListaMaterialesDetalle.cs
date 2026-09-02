namespace Vercom.Models;

public partial class ListaMaterialesDetalle
{
    public Guid Id { get; set; }

    public Guid ListaMaterialesId { get; set; }

    public Guid ProductoInsumoId { get; set; }

    public decimal CantidadRequerida { get; set; }

    public decimal PorcentajeMerma { get; set; }

    public virtual ListaMateriale ListaMateriales { get; set; } = null!;

    public virtual Producto ProductoInsumo { get; set; } = null!;

}
