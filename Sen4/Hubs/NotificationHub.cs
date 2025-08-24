using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Sen4.Hubs;


[Authorize]
public class NotificationHub: Hub
{
    public override async Task OnConnectedAsync()
    {
        var user = Context.UserIdentifier;
        Console.WriteLine($"User connect to hub:, {user}");
        await base.OnConnectedAsync();
    }
}