using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class Consecutivo
{
    public int Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid? SucursalId { get; set; }

    public string TipoDocumento { get; set; } = null!;

    public string Serie { get; set; } = null!;

    public long UltimoNumero { get; set; }

    public short LongitudPadding { get; set; }

    public DateTimeOffset ActualizadoEn { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual Sucursal? Sucursal { get; set; }
}
