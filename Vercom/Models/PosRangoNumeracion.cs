using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class PosRangoNumeracion
{
    public Guid Id { get; set; }

    public Guid DispositivoPosId { get; set; }

    public string TipoDocumento { get; set; } = null!;

    public string Serie { get; set; } = null!;

    public long NumeroDesde { get; set; }

    public long NumeroHasta { get; set; }

    public long NumeroSiguienteLocal { get; set; }

    public DateTimeOffset AsignadoEn { get; set; }

    public bool Agotado { get; set; }

    public virtual DispositivoPo DispositivoPos { get; set; } = null!;
}
