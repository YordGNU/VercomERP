namespace Vercom.Models;

public partial class ListaPrecio
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Canal { get; set; } = null!;

    public DateOnly VigenteDesde { get; set; }

    public DateOnly? VigenteHasta { get; set; }

    public bool Activa { get; set; }

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<ListaPrecioDetalle> ListaPrecioDetalles { get; set; } = new List<ListaPrecioDetalle>();
}
