namespace Vercom.Models;

public partial class VAuditoriaReversione
{
    public long Id { get; set; }

    public Guid? UsuarioId { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string EsquemaTabla { get; set; } = null!;

    public string? RegistroId { get; set; }

    public string? ValoresAnteriores { get; set; }

    public string? ValoresNuevos { get; set; }

    public string Canal { get; set; } = null!;

    public DateTimeOffset OcurridoEn { get; set; }
}
