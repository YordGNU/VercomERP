using System.Security.Claims;
using Vercom.Helpers;
using Vercom.Models;

namespace Vercom.Services
{
    public class AuditService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;

        public AuditService(IHttpContextAccessor httpContextAccessor, AppDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task LogAsync(
            string accion,
            string esquemaTabla,
            string registroId,
            string? valoresAnteriores = null,
            string? valoresNuevos = null,
            string? canal = null,
            Guid? usuarioId = null,
            string? nombreUsuario = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                // Usuario
                Guid? userId = usuarioId;
                string? nombreUser = nombreUsuario;

                if (userId == null && httpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (Guid.TryParse(userIdClaim, out var uid))
                        userId = uid;
                    nombreUser = httpContext.User.Identity?.Name ?? "SISTEMA";
                }

                if (string.IsNullOrEmpty(nombreUser))
                    nombreUser = "SISTEMA";

                // ✅ CANAL SANITIZADO (el fix clave)
                var canalRaw = canal
                    ?? httpContext?.Request?.Headers["X-Channel"].FirstOrDefault();

                var channel = AuditConstants.NormalizeCanal(canalRaw);

                var ip = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";

                var audit = new Auditorium
                {
                    UsuarioId = userId,
                    NombreUsuario = nombreUser,
                    Accion = accion,
                    EsquemaTabla = esquemaTabla,
                    RegistroId = registroId,
                    ValoresAnteriores = valoresAnteriores,
                    ValoresNuevos = valoresNuevos,
                    IpOrigen = ip,
                    Canal = channel,        // ← Siempre ERP/API/POS
                    OcurridoEn = DateTimeOffset.Now
                };

                _context.Auditoria.Add(audit);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR AUDITORÍA] {ex.Message}");
            }
        }
    }
}
