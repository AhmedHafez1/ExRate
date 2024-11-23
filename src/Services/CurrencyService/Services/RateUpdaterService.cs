using CurrencyService.Contracts;
using CurrencyService.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CurrencyService.Services;

public class RateUpdaterService : BackgroundService
{
    private readonly IExchangeRateService _rateService;
    private readonly IHubContext<RatesHub> _hubContext;
    private readonly ILogger<RateUpdaterService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(30);

    public RateUpdaterService(
        IExchangeRateService rateService,
        IHubContext<RatesHub> hubContext,
        ILogger<RateUpdaterService> logger
    )
    {
        _rateService = rateService;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Update the rates and push the new data to clients
            var updatedRates = await _rateService.GetExchangeRatesAsync("USD");
            await _hubContext.Clients.All.SendAsync("ReceiveRates", updatedRates);

            _logger.Log(LogLevel.Information, $"Last Updated data for USD at {updatedRates.Date}");

            // Wait for the next update
            await Task.Delay(_updateInterval, stoppingToken);
        }
    }
}
