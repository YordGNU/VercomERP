using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class Auditorium
{
    public long Id { get; set; }

    public Guid? UsuarioId { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Accion { get; set; } = null!;

    public string EsquemaTabla { get; set; } = null!;

    public string? RegistroId { get; set; }

    public string? ValoresAnteriores { get; set; }

    public string? ValoresNuevos { get; set; }

    public string? IpOrigen { get; set; }

    public string Canal { get; set; } = null!;

    public Guid? DispositivoId { get; set; }

    public DateTimeOffset OcurridoEn { get; set; }

    public virtual DispositivoPo? Dispositivo { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
