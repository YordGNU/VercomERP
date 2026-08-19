using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class ApiLog
{
    public long Id { get; set; }

    public Guid? ApiClienteId { get; set; }

    public string MetodoHttp { get; set; } = null!;

    public string Endpoint { get; set; } = null!;

    public short? CodigoRespuesta { get; set; }

    public int? DuracionMs { get; set; }

    public string? IpOrigen { get; set; }

    public DateTimeOffset OcurridoEn { get; set; }

    public virtual ApiCliente? ApiCliente { get; set; }
}
