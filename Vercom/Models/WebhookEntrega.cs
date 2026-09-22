namespace Vercom.Models;

public partial class WebhookEntrega
{
    public Guid Id { get; set; }

    public Guid SuscripcionId { get; set; }

    public string PayloadJson { get; set; } = null!;

    public short IntentoNumero { get; set; }

    public short? CodigoRespuestaHttp { get; set; }

    public bool Exitoso { get; set; }

    public DateTimeOffset? ProximoReintentoEn { get; set; }

    public DateTimeOffset EnviadoEn { get; set; }

    public virtual WebhookSuscripcion Suscripcion { get; set; } = null!;
    public string MensajeError { get; internal set; }
}
