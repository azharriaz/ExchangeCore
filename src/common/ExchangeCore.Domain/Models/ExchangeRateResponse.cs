namespace ExchangeCore.Domain.Models;

public class ExchangeRateResponse
{
    public double Amount { get; set; }
    public string Base { get; set; }
    public DateOnly Date { get; set; }
    public Dictionary<string, double> Rates { get; set; }
}
