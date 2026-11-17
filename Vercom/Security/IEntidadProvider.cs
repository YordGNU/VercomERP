using System.Security.Claims;

namespace VERCOM.Security;

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
            // Sin contexto HTTP no hay usuario al que aislar: son procesos internos (worker de
            // sincronización POS, tareas de fondo). Devolver false vaciaba TODAS las consultas
            // por el filtro global (EntidadId == Guid.Empty) y, entre otros efectos, el POS
            // intentaba crear un periodo contable ya existente y la venta fallaba con una
            // clave duplicada. Los procesos internos ven todas las filas, como el usuario master.
            if (user == null) return true;

            // Prioridad: Rol MASTER, Fallback: Nombre de usuario 'master'
            return user.IsInRole("MASTER") || user.Identity?.Name == "master";
        }
    }
}
