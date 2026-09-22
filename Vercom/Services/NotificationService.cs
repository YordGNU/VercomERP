using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Vercom.Hubs;
using Vercom.Models;

namespace Vercom.Services;

public interface INotificationService
{
    Task NotifyEntityAsync(Guid entidadId, string title, string message, string type = "info");
    Task NotifyUserAsync(Guid userId, string title, string message, string type = "info");
    Task NotifyMasterAsync(string title, string message, string type = "warning");

    // Webhooks (Iteración 7)
    Task EnqueueEventAsync(string eventName, object payload, Guid? entidadId = null);
    Task<int> ProcessPendingWebhooksAsync(int batchSize = 10);
}

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly AppDbContext _context;
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(15) };
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<NotificationHub> hubContext, AppDbContext context, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _context = context;
        _logger = logger;
    }

    public async Task NotifyEntityAsync(Guid entidadId, string title, string message, string type = "info")
    {
        // Persistir en la bandeja de la entidad (historial y contador de no leídas).
        _context.Notificaciones.Add(new Notificacion
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            UsuarioId = null,
            Titulo = title,
            Mensaje = message,
            Tipo = type,
            Leida = false
        });
        try { await _context.SaveChangesAsync(); }
        catch (Exception ex) { _logger.LogWarning(ex, "No se pudo persistir la notificación de la entidad {EntidadId}.", entidadId); }

        await _hubContext.Clients.Group(entidadId.ToString())
            .SendAsync("ReceiveNotification", new { title, message, type, time = DateTime.Now.ToString("HH:mm") });
    }

    public async Task NotifyUserAsync(Guid userId, string title, string message, string type = "info")
    {
        _context.Notificaciones.Add(new Notificacion
        {
            Id = Guid.NewGuid(),
            EntidadId = null,
            UsuarioId = userId,
            Titulo = title,
            Mensaje = message,
            Tipo = type,
            Leida = false
        });
        try { await _context.SaveChangesAsync(); }
        catch (Exception ex) { _logger.LogWarning(ex, "No se pudo persistir la notificación del usuario {UserId}.", userId); }

        // En SignalR usualmente se usa el UserId si está configurado el Provider de ID
        await _hubContext.Clients.User(userId.ToString())
            .SendAsync("ReceiveNotification", new { title, message, type, time = DateTime.Now.ToString("HH:mm") });
    }

    public async Task NotifyMasterAsync(string title, string message, string type = "warning")
    {
        _context.Notificaciones.Add(new Notificacion
        {
            Id = Guid.NewGuid(),
            EntidadId = null,
            UsuarioId = null,
            Titulo = title,
            Mensaje = message,
            Tipo = type,
            Leida = false
        });
        try { await _context.SaveChangesAsync(); }
        catch (Exception ex) { _logger.LogWarning(ex, "No se pudo persistir la notificación del maestro."); }

        // El grupo "MASTER" es para alertas globales
        await _hubContext.Clients.Group("MASTER")
            .SendAsync("ReceiveNotification", new { title, message, type, time = DateTime.Now.ToString("HH:mm") });
    }

    // ============================================================
    // WEBHOOKS (Iteración 7: Seguridad HMAC-SHA256)
    // ============================================================
    public async Task EnqueueEventAsync(string eventName, object payload, Guid? entidadId = null)
    {
        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);

        // Buscar suscripciones activas para este evento
        var subscriptions = await _context.WebhookSuscripcions
            .Where(s => s.Evento == eventName && s.Activo && (entidadId == null || s.ApiCliente.EntidadId == entidadId))
            .ToListAsync();

        foreach (var sub in subscriptions)
        {
            var delivery = new WebhookEntrega
            {
                Id = Guid.NewGuid(),
                SuscripcionId = sub.Id,
                PayloadJson = payloadJson,
                ProximoReintentoEn = DateTimeOffset.Now,
                IntentoNumero = 0,
                Exitoso = false
            };
            _context.WebhookEntregas.Add(delivery);
        }

        if (subscriptions.Any()) await _context.SaveChangesAsync();
    }

    public async Task<int> ProcessPendingWebhooksAsync(int batchSize = 10)
    {
        var pending = await _context.WebhookEntregas
            .Include(e => e.Suscripcion)
            .Where(e => !e.Exitoso && e.ProximoReintentoEn <= DateTimeOffset.Now)
            .Take(batchSize)
            .ToListAsync();

        int sentCount = 0;

        foreach (var delivery in pending)
        {
            var sub = delivery.Suscripcion;

            // 1. Calcular Firma HMAC-SHA256
            var secret = sub.SecretoFirmaHash ?? "VercomDefaultSecret";
            var signature = "";
            using (var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret)))
            {
                var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(delivery.PayloadJson));
                signature = Convert.ToHexString(hash).ToLowerInvariant();
            }

            // 2. Preparar Request
            using var request = new HttpRequestMessage(HttpMethod.Post, sub.UrlDestino);
            request.Content = new StringContent(delivery.PayloadJson, System.Text.Encoding.UTF8, "application/json");
            request.Headers.Add("X-Webhook-Signature", $"sha256={signature}");

            try
            {
                var response = await _httpClient.SendAsync(request);
                delivery.CodigoRespuestaHttp = (short?)response.StatusCode;
                delivery.Exitoso = response.IsSuccessStatusCode;
                delivery.ProximoReintentoEn = delivery.Exitoso ? null : DateTimeOffset.Now.AddMinutes(Math.Pow(2, delivery.IntentoNumero));
            }
            catch (Exception ex)
            {
                delivery.MensajeError = ex.Message;
                delivery.Exitoso = false;
                delivery.ProximoReintentoEn = DateTimeOffset.Now.AddMinutes(Math.Pow(2, delivery.IntentoNumero));
                _logger.LogWarning(ex, "Error enviando webhook {Id}", delivery.Id);
            }

            delivery.IntentoNumero++;
            if (delivery.IntentoNumero > 5 && !delivery.Exitoso)
            {
                delivery.ProximoReintentoEn = null; // Desistir tras 5 fallos
            }

            if (delivery.Exitoso) sentCount++;
        }

        await _context.SaveChangesAsync();
        return sentCount;
    }
}
