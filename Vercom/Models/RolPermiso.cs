namespace VERCOM.Models;

public partial class RolPermiso
{
    public int RolId { get; set; }
    public int PermisoId { get; set; }
    
    public virtual Rol Rol { get; set; } = null!;
    public virtual Permiso Permiso { get; set; } = null!;
}