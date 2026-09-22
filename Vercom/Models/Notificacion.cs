using System;

namespace Vercom.Models;

public partial class Notificacion
{
    public Guid Id { get; set; }

    public Guid? EntidadId { get; set; }

    public Guid? UsuarioId { get; set; }

    public string Titulo { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    // success | info | warning | danger
    public string Tipo { get; set; } = "info";

    public string? Enlace { get; set; }

    public bool Leida { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Entidad? Entidad { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
