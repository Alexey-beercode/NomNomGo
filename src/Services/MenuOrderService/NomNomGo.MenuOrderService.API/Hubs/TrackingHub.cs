using Microsoft.AspNetCore.SignalR;

namespace NomNomGo.MenuOrderService.API.Hubs;

public class TrackingHub : Hub
{
    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    public async Task JoinCourierGroup(string courierId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"courier_{courierId}");
    }

    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    public async Task LeaveCourierGroup(string courierId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"courier_{courierId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}