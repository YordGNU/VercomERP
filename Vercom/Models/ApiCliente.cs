using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class ApiCliente
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public string ClientId { get; set; } = null!;

    public string ClientSecretHash { get; set; } = null!;

    public string? Scopes { get; set; }

    public bool Activo { get; set; }

    public Guid? CreadoPor { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public DateTimeOffset? RevocadoEn { get; set; }

    public virtual ICollection<ApiLog> ApiLogs { get; set; } = new List<ApiLog>();

    public virtual ApiRateLimit? ApiRateLimit { get; set; }

    public virtual ICollection<ApiToken> ApiTokens { get; set; } = new List<ApiToken>();

    public virtual Usuario? CreadoPorNavigation { get; set; }

    public virtual ICollection<DispositivoPo> DispositivoPos { get; set; } = new List<DispositivoPo>();

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<WebhookSuscripcion> WebhookSuscripcions { get; set; } = new List<WebhookSuscripcion>();
}
