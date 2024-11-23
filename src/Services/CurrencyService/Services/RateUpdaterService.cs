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
            // Update the rates and push the new data to clients
            foreach (var currency in currencies)
            {
                var updatedRates = await _rateService.GetExchangeRatesAsync(currency);
                await _hubContext.Clients.Group(currency).SendAsync(currency, updatedRates);

                _logger.LogInformation($"Last Updated data for {currency} at {updatedRates.Date}");
            }

            // Wait for the next update
            await Task.Delay(_updateInterval, stoppingToken);
        }
    }
}
