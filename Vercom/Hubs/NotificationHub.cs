using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Vercom.Hubs;

public class NotificationHub : Hub
{
    public async Task SendNotification(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", user, message);
    }

    public override async Task OnConnectedAsync()
    {
        // 1. Unir a grupo de Entidad
        var entidadId = Context.GetHttpContext()?.User.FindFirst("EntidadId")?.Value;
        if (!string.IsNullOrEmpty(entidadId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, entidadId);
        }

        // 2. Unir a grupo MASTER si es el usuario maestro
        if (Context.User?.Identity?.Name == "master")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "MASTER");
        }

        await base.OnConnectedAsync();
    }
}
