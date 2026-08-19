using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class Permiso
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Modulo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Rol> Rols { get; set; } = new List<Rol>();
}
