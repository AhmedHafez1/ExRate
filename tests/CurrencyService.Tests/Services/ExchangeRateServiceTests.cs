using System.Net;
using System.Text.Json;
using CurrencyService.DTOs;
using CurrencyService.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CurrencyService.Tests.Services;

public class ExchangeRateServiceTests
{
    private readonly MockHttpMessageHandler _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly Mock<ILogger<ExchangeRateService>> _loggerMock;
    private readonly ExchangeRateService _exchangeRateService;

    public ExchangeRateServiceTests()
    {
        _httpMessageHandlerMock = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_httpMessageHandlerMock);
        _configurationMock = new Mock<IConfiguration>();
        _memoryCacheMock = new Mock<IMemoryCache>();
        _loggerMock = new Mock<ILogger<ExchangeRateService>>();

        // Set up configuration
        _configurationMock
            .Setup(config => config["CurrencyApiUrl"])
            .Returns("https://api.fxratesapi.com/latest");

        _exchangeRateService = new ExchangeRateService(
            _httpClient,
            _configurationMock.Object,
            _memoryCacheMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetExchangeRatesAsync_ShouldFetchAndCacheData_WhenCacheIsNotAvailable()
    {
        // Arrange
        var baseCurrency = "USD";
        var fetchedRates = new ExchangeRatesDto
        {
            Base = baseCurrency,
            Rates = new Dictionary<string, decimal> { { "EUR", 0.85m }, { "GBP", 0.75m } }
        };

        object cachedObject = null!;
        _memoryCacheMock
            .Setup(cache => cache.TryGetValue(baseCurrency, out cachedObject!))
            .Returns(false);

        // Simulate API response
        _httpMessageHandlerMock.SendAsyncFunction = (request, token) =>
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(fetchedRates))
            };
        };

        _memoryCacheMock
            .Setup(cache => cache.CreateEntry(baseCurrency))
            .Returns(Mock.Of<ICacheEntry>());

        // Act
        var result = await _exchangeRateService.GetExchangeRatesAsync(baseCurrency);

        // Assert
        Assert.Equal(fetchedRates.Base, result.Base);
        Assert.Equal(fetchedRates.Rates, result.Rates);
        _memoryCacheMock.Verify(cache => cache.CreateEntry(baseCurrency), Times.Once);
    }
}
