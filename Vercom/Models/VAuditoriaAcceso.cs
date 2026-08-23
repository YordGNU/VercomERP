namespace Vercom.Models;

public partial class VAuditoriaAcceso
{
    public long Id { get; set; }

    public Guid? UsuarioId { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Accion { get; set; } = null!;

    public string? IpOrigen { get; set; }

    public string Canal { get; set; } = null!;

    public DateTimeOffset OcurridoEn { get; set; }
}
