using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class PosVentaPendiente
{
    public Guid Id { get; set; }

    public Guid DispositivoPosId { get; set; }

    public Guid SesionCajaPosId { get; set; }

    public string IdempotencyKey { get; set; } = null!;

    public string PayloadJson { get; set; } = null!;

    public DateTimeOffset FechaVentaLocal { get; set; }

    public DateTimeOffset FechaRecibidoServidor { get; set; }

    public string Estado { get; set; } = null!;

    public Guid? FacturaId { get; set; }

    public string? MensajeError { get; set; }

    public short IntentosProcesamiento { get; set; }

    public DateTimeOffset? ProcesadoEn { get; set; }

    public virtual DispositivoPo DispositivoPos { get; set; } = null!;

    public virtual FacturaVentum? Factura { get; set; }

    public virtual SesionCajaPo SesionCajaPos { get; set; } = null!;
}
