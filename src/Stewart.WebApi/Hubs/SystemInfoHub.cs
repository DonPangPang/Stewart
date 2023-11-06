using Microsoft.AspNetCore.SignalR;
using Stewart.Shared;

namespace Stewart.WebApi.Hubs;

public interface INotifyHubClient
{
    Task ReceiveMessage(string user, ServerInfo message);

    Task ReceiveGroupMessage(ServerInfo message);
}

public class SystemInfoHub : Hub<INotifyHubClient>
{
    public SystemInfoHub()
    {
    }

    public async Task SendMessage(string user, ServerInfo message)
    {
        await Clients.All.ReceiveMessage(user, message);
    }
}