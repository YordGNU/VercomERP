namespace Vercom.Models;

public partial class ApiRateLimit
{
    public Guid ApiClienteId { get; set; }

    public int SolicitudesPorMinuto { get; set; }

    public DateTimeOffset ActualizadoEn { get; set; }

    public virtual ApiCliente ApiCliente { get; set; } = null!;
}
