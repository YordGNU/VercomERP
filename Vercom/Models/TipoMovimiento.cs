namespace Vercom.Models;

public partial class TipoMovimiento
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Naturaleza { get; set; } = null!;

    public bool AfectaCosto { get; set; }

    public virtual ICollection<MovimientoInventario> MovimientoInventarios { get; set; } = new List<MovimientoInventario>();
}
