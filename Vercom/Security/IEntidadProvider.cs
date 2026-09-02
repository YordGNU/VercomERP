using System.Security.Claims;

namespace Vercom.Security;

public interface IEntidadProvider
{
    Guid CurrentEntidadId { get; }
    Guid CurrentUsuarioId { get; }
    Guid? CurrentSucursalId { get; }
    bool IsMaster { get; }
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

    public Guid? CurrentSucursalId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("SucursalId")?.Value;
            return Guid.TryParse(claim, out var guid) ? guid : null;
        }
    }

    public bool IsMaster
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return false;

            // Prioridad: Rol MASTER, Fallback: Nombre de usuario 'master'
            return user.IsInRole("MASTER") || user.Identity?.Name == "master";
        }
    }
}
