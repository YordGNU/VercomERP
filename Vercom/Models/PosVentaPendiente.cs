using System;
using System.Collections.Generic;

namespace Vercom.Models;

/// <summary>
/// RNF-02/RNF-50: la app POS crea el registro localmente con idempotency_key propio y hace upsert al reconectar. El worker de sincronización procesa PENDIENTE -&gt; crea factura_venta -&gt; marca PROCESADO. Reintentos seguros gracias a la clave única (dispositivo, idempotency_key).
/// </summary>
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
