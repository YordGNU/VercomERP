using Microsoft.AspNetCore.SignalR;
using Vercom.Hubs;

namespace Vercom.Services;

public interface INotificationService
{
    Task NotifyEntityAsync(Guid entidadId, string title, string message, string type = "info");
    Task NotifyUserAsync(Guid userId, string title, string message, string type = "info");
    Task NotifyMasterAsync(string title, string message, string type = "warning");
}

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyEntityAsync(Guid entidadId, string title, string message, string type = "info")
    {
        await _hubContext.Clients.Group(entidadId.ToString())
            .SendAsync("ReceiveNotification", new { title, message, type, time = DateTime.Now.ToString("HH:mm") });
    }

    public async Task NotifyUserAsync(Guid userId, string title, string message, string type = "info")
    {
        // En SignalR usualmente se usa el UserId si está configurado el Provider de ID
        await _hubContext.Clients.User(userId.ToString())
            .SendAsync("ReceiveNotification", new { title, message, type, time = DateTime.Now.ToString("HH:mm") });
    }

    public async Task NotifyMasterAsync(string title, string message, string type = "warning")
    {
        // El grupo "MASTER" es para alertas globales
        await _hubContext.Clients.Group("MASTER")
            .SendAsync("ReceiveNotification", new { title, message, type, time = DateTime.Now.ToString("HH:mm") });
    }
}
