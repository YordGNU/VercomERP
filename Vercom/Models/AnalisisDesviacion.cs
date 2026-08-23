namespace Vercom.Models;

public partial class AnalisisDesviacion
{
    public Guid Id { get; set; }

    public Guid OrdenProduccionId { get; set; }

    public string Componente { get; set; } = null!;

    public decimal CostoEstandar { get; set; }

    public decimal CostoReal { get; set; }

    public decimal? Desviacion { get; set; }

    public DateTimeOffset AnalizadoEn { get; set; }

    public virtual OrdenProduccion OrdenProduccion { get; set; } = null!;
}
