using System;

namespace Vercom.Models;

public partial class Feedback
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid UsuarioId { get; set; }

    public string Tipo { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    public string? MetadataTecnica { get; set; }

    public string Estado { get; set; } = null!;

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
