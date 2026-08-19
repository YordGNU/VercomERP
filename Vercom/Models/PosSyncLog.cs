using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class PosSyncLog
{
    public Guid Id { get; set; }

    public Guid DispositivoPosId { get; set; }

    public string TipoSync { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public int RegistrosProcesados { get; set; }

    public string Estado { get; set; } = null!;

    public string? DetalleError { get; set; }

    public DateTimeOffset IniciadoEn { get; set; }

    public DateTimeOffset? FinalizadoEn { get; set; }

    public virtual DispositivoPo DispositivoPos { get; set; } = null!;
}
