using CurrencyService.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace CurrencyService.Hubs;

public class RatesHub : Hub
{
    public async Task SendRateUpdates(ExchangeRatesDto rates)
    {
        await Clients.All.SendAsync("ReceiveRates", rates);
    }
}
