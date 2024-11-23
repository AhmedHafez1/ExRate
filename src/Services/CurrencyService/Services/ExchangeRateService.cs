using System.Text.Json;
using CurrencyService.Contracts;
using CurrencyService.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyService.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    private readonly string[] _currencies;
    private readonly IMemoryCache _memoryCache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);

    public ExchangeRateService(
        HttpClient httpClient,
        IConfiguration config,
        IMemoryCache memoryCache
    )
    {
        _httpClient = httpClient;
        _apiUrl = config["CurrencyApiUrl"]!;
        _currencies = config.GetSection("Currencies").Get<string[]>();
        _memoryCache = memoryCache;
    }

    public async Task<ExchangeRatesDto> GetExchangeRatesAsync(string baseCurrency)
    {
        // Check if the exchange rates are already cached
        if (_memoryCache.TryGetValue(baseCurrency, out ExchangeRatesDto cachedRates))
        {
            return cachedRates;
        }

        // If not cached, fetch from API and parse
        var exchangeRatesDto = await _httpClient.GetFromJsonAsync<ExchangeRatesDto>(
            $"{_apiUrl}?base={baseCurrency}&currencies={string.Join(",", _currencies)}"
        );

        // Cache the fetched data with expiration time
        _memoryCache.Set(baseCurrency, exchangeRatesDto, CacheDuration);

        return exchangeRatesDto;
    }
}
