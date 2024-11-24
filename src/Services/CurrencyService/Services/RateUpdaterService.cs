using System.Net;
using CurrencyService.Contracts;
using CurrencyService.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CurrencyService.Services;

public class RateUpdaterService : BackgroundService
{
    private readonly IExchangeRateService _rateService;
    private readonly IHubContext<RatesHub> _hubContext;
    private readonly ILogger<RateUpdaterService> _logger;
    private readonly IConfiguration _config;
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(30);

    public RateUpdaterService(
        IExchangeRateService rateService,
        IHubContext<RatesHub> hubContext,
        ILogger<RateUpdaterService> logger,
        IConfiguration config
    )
    {
        _rateService = rateService;
        _hubContext = hubContext;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var currencies = _config.GetSection("Currencies").Get<string[]>();

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var currency in currencies)
            {
                try
                {
                    var updatedRates = await _rateService.GetExchangeRatesAsync(currency);
                    await _hubContext.Clients.Group(currency).SendAsync(currency, updatedRates);
                    _logger.LogInformation(
                        $"Last Updated data for {currency} at {DateTime.UtcNow}"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating rates for {currency}");
                }

                // Add a short delay between requests to avoid overloading the API
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }

            // Wait for the next update
            await Task.Delay(_updateInterval, stoppingToken);
        }
    }
}
