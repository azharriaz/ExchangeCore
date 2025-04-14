namespace ExchangeCore.Domain.ConfigOptions.CurrencyConverter;

public class CurrencyConverterOptions
{
    public static string SectionName => "CurrencyConverter";
    public string BaseUrl { get; set; }
    public string[] CurrencyExclusions { get; set; }
    public RetryPolicyOptions RetryPolicy { get; set; }
    public CircuitBreakerPolicy CircuitBreakerPolicy { get; set; }
}

public class RetryPolicyOptions
{
    public int MaxRetries { get; set; }
    public int RetryIntervalSeconds { get; set; }
    public int TimeoutSeconds { get; set; }
}

public class CircuitBreakerPolicy
{
    public int CircuitBreakerDurationSeconds { get; set; }
    public int MinimumRequestsThrouput { get; set; }
}