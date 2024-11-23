using CurrencyService.Contracts;
using CurrencyService.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrencyController : ControllerBase
{
    private readonly IExchangeRateService _exchangeRateService;

    public CurrencyController(IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }

    [HttpGet]
    public async Task<ActionResult<ExchangeRatesDto>> GetExchangeRates(string baseCurrency = "USD")
    {
        var rates = await _exchangeRateService.GetExchangeRatesAsync(baseCurrency);
        return Ok(rates);
    }
}
