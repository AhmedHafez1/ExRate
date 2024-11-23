namespace CurrencyService.DTOs;

public class ExchangeRatesDto
{
    public string Base { get; set; }
    public DateTime Date { get; set; }
    public Dictionary<string, decimal> Rates { get; set; }
}
