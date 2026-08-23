namespace Vercom.Models;

public partial class CentroCosto
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public Guid? SucursalId { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<AsientoDetalle> AsientoDetalles { get; set; } = new List<AsientoDetalle>();

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<PresupuestoLinea> PresupuestoLineas { get; set; } = new List<PresupuestoLinea>();

    public virtual Sucursal? Sucursal { get; set; }
}
