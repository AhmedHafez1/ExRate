using CurrencyService.Config;
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
            foreach (var currency in CurrencyConfig.RealTimeCurrencies)
            {
                try
                {
                    var updatedRates = await _rateService.GetExchangeRatesAsync(currency);
                    await _hubContext.Clients.Group(currency).SendAsync(currency, updatedRates);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating rates for {currency}");
                }

                // Add a short delay between requests to avoid overloading the API
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }

            // Wait for the next update
            await Task.Delay(_updateInterval, stoppingToken);
        }
    }
}
