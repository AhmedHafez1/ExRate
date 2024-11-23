using CurrencyService.DTOs;

namespace CurrencyService.Contracts;

public interface IExchangeRateService
{
    Task<ExchangeRatesDto> GetExchangeRatesAsync(string baseCurrency);
}
