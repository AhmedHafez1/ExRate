using Microsoft.AspNetCore.SignalR;

namespace CurrencyService.Hubs;

public class RatesHub : Hub
{
    public async Task JoinGroup(string currency)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, currency);
    }

    public async Task LeaveGroup(string currency)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, currency);
    }
}
