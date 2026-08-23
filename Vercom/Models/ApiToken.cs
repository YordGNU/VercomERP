namespace Vercom.Models;

public partial class ApiToken
{
    public Guid Id { get; set; }

    public Guid ApiClienteId { get; set; }

    public string TokenHash { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public DateTimeOffset EmitidoEn { get; set; }

    public DateTimeOffset ExpiraEn { get; set; }

    public bool Revocado { get; set; }

    public string? IpOrigen { get; set; }

    public virtual ApiCliente ApiCliente { get; set; } = null!;
}
