using VERCOM.Services;

namespace Vercom.Tests.Infrastructure;

/// <summary>Notificaciones en memoria: servicios que notifican sin depender de SignalR.</summary>
public sealed class NotificacionNula : INotificationService
{
    public Task NotifyEntityAsync(Guid entidadId, string title, string message, string type = "info") => Task.CompletedTask;

    public Task NotifyUserAsync(Guid userId, string title, string message, string type = "info") => Task.CompletedTask;

    public Task NotifyMasterAsync(string title, string message, string type = "warning") => Task.CompletedTask;

    public Task EnqueueEventAsync(string eventName, object payload, Guid? entidadId = null) => Task.CompletedTask;

    public Task<int> ProcessPendingWebhooksAsync(int batchSize = 10) => Task.FromResult(0);
}
