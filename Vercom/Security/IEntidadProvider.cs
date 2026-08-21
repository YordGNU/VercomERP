using System.Security.Claims;

namespace Vercom.Security;

public interface IEntidadProvider
{
    Guid CurrentEntidadId { get; }
    Guid CurrentUsuarioId { get; }
}

public class HttpContextEntidadProvider : IEntidadProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextEntidadProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid CurrentEntidadId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("EntidadId")?.Value;
            return Guid.TryParse(claim, out var guid) ? guid : Guid.Empty;
        }
    }

    public Guid CurrentUsuarioId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var guid) ? guid : Guid.Empty;
        }
    }
}
