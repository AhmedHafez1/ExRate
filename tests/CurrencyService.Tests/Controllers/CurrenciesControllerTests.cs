using CurrencyService.Contracts;
using CurrencyService.Controllers;
using CurrencyService.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CurrencyService.Tests.Controllers;

public class CurrenciesControllerTests
{
    private readonly Mock<IExchangeRateService> _mockExchangeRateService;
    private readonly CurrenciesController _controller;

    public CurrenciesControllerTests()
    {
        _mockExchangeRateService = new Mock<IExchangeRateService>();
        _controller = new CurrenciesController(_mockExchangeRateService.Object);
    }

    [Fact]
    public async Task GetExchangeRates_ShouldReturnOkWithExchangeRatesDto_WhenCalledWithValidBaseCurrency()
    {
        // Arrange
        var baseCurrency = "EUR";
        var exchangeRatesDto = new ExchangeRatesDto
        {
            Base = baseCurrency,
            Rates = new Dictionary<string, decimal> { { "USD", 1.05m }, { "JYN", 140.21m } }
        };
        _mockExchangeRateService
            .Setup(service => service.GetExchangeRatesAsync(baseCurrency))
            .ReturnsAsync(exchangeRatesDto);

        // Act
        var result = await _controller.GetExchangeRates(baseCurrency);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ExchangeRatesDto>(okResult.Value);
        Assert.Equal(baseCurrency, returnedDto.Base);
        Assert.Equal(exchangeRatesDto.Rates, returnedDto.Rates);

        _mockExchangeRateService.Verify(
            service => service.GetExchangeRatesAsync(baseCurrency),
            Times.Once
        );
    }

    [Fact]
    public async Task GetExchangeRates_ShouldReturnDefaultBaseCurrency_WhenBaseCurrencyIsNotProvided()
    {
        // Arrange
        var defaultBaseCurrency = "USD";
        var exchangeRatesDto = new ExchangeRatesDto
        {
            Base = defaultBaseCurrency,
            Rates = new Dictionary<string, decimal> { { "EUR", 0.85m }, { "GBP", 0.75m } }
        };
        _mockExchangeRateService
            .Setup(service => service.GetExchangeRatesAsync(defaultBaseCurrency))
            .ReturnsAsync(exchangeRatesDto);

        // Act
        var result = await _controller.GetExchangeRates();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ExchangeRatesDto>(okResult.Value);
        Assert.Equal(defaultBaseCurrency, returnedDto.Base);

        _mockExchangeRateService.Verify(
            service => service.GetExchangeRatesAsync(defaultBaseCurrency),
            Times.Once
        );
    }
}
