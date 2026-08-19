using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class BackupLog
{
    public Guid Id { get; set; }

    public string Tipo { get; set; } = null!;

    public string RutaArchivo { get; set; } = null!;

    public long? TamanoBytes { get; set; }

    public string Estado { get; set; } = null!;

    public string? MensajeError { get; set; }

    public DateTimeOffset IniciadoEn { get; set; }

    public DateTimeOffset? FinalizadoEn { get; set; }
}
