namespace Vercom.Models;

public partial class Rol
{
    public int Id { get; set; }

    public Guid? EntidadId { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool EsSistema { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<UsuarioRol> UsuarioRols { get; set; } = new List<UsuarioRol>();

    public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();

}
