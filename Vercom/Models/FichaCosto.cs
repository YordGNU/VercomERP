namespace Vercom.Models;

/// <summary>
/// RNF-40: versionada para permitir redefinición ágil ante cambios de precios de insumos.
/// </summary>
public partial class FichaCosto
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid ProductoId { get; set; }

    public int Version { get; set; }

    public DateOnly VigenteDesde { get; set; }

    public DateOnly? VigenteHasta { get; set; }

    public decimal CostoMateriaPrima { get; set; }

    public decimal CostoManoObra { get; set; }

    public decimal GastosIndirectos { get; set; }

    public decimal? CostoTotalUnitario { get; set; }

    public decimal? MargenPorcentaje { get; set; }

    public decimal? PrecioSugerido { get; set; }

    public string Estado { get; set; } = null!;

    public Guid? CreadoPor { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Usuario? CreadoPorNavigation { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<OrdenProduccion> OrdenProduccions { get; set; } = new List<OrdenProduccion>();

    public virtual Producto Producto { get; set; } = null!;
}
