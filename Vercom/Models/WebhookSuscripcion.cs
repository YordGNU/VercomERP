namespace Vercom.Models;

public partial class WebhookSuscripcion
{
    public Guid Id { get; set; }

    public Guid ApiClienteId { get; set; }

    public string Evento { get; set; } = null!;

    public string UrlDestino { get; set; } = null!;

    public string SecretoFirmaHash { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual ApiCliente ApiCliente { get; set; } = null!;

    public virtual ICollection<WebhookEntrega> WebhookEntregas { get; set; } = new List<WebhookEntrega>();
}
