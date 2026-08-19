using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class TipoComprobante
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public virtual ICollection<AsientoContable> AsientoContables { get; set; } = new List<AsientoContable>();
}
