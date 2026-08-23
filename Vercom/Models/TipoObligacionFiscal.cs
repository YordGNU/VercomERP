namespace Vercom.Models;

public partial class TipoObligacionFiscal
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Periodicidad { get; set; } = null!;

    public decimal? TasaActual { get; set; }

    public string? BaseLegal { get; set; }

    public virtual ICollection<DeclaracionJuradum> DeclaracionJurada { get; set; } = new List<DeclaracionJuradum>();
}
