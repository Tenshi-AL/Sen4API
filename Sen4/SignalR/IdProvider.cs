using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Sen4.SignalR;

public class IdProvider: IUserIdProvider
{
    public virtual string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
