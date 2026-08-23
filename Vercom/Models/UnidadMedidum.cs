namespace Vercom.Models;

public partial class UnidadMedidum
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public bool EsFraccionable { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
