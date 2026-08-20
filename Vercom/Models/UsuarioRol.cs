using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class UsuarioRol
{
    public Guid UsuarioId { get; set; }

    public int RolId { get; set; }

    public Guid SucursalId { get; set; }

    public DateTimeOffset AsignadoEn { get; set; }

    public Guid? AsignadoPor { get; set; }

    public virtual Usuario? AsignadoPorNavigation { get; set; }

    public virtual Rol Rol { get; set; } = null!;

    public virtual Sucursal Sucursal { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
