using ExchangeCore.Application.Common.Interfaces;
using ExchangeCore.Application.CurrencyConverter.Queries.CurrencyConversion;
using ExchangeCore.Application.CurrencyConverter.Queries.GetHistoricRates;
using ExchangeCore.Domain.Common;
using ExchangeCore.Domain.Models;
using Newtonsoft.Json;

namespace ExchangeCore.Infrastructure.Services;

/// <summary>
/// Service for interacting with the currency converter API.
/// </summary>
public class CurrencyConverterService : ICurrencyConverterService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyConverterService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The factory to create HttpClient instances.</param>
    public CurrencyConverterService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.CURRENCY_CONVERTER_CLIENT);
    }

    /// <summary>
    /// Converts an amount from one currency to another.
    /// </summary>
    /// <param name="command">The command containing the conversion details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The exchange rate response with the conversion result.</returns>
    public async Task<ExchangeRateResponse> ConvertCurrencyAsync(ConvertCurrencyQuery command, CancellationToken cancellationToken)
    {
        var requestUri = string.Format(Constants.CURRENCY_CONVERTER_URL_CONVERSION, command.Amount, command.FromCurrency, command.ToCurrency);

        var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<ExchangeRateResponse>(content);
    }

    /// <summary>
    /// Retrieves the latest exchange rates for a specified base currency.
    /// </summary>
    /// <param name="baseCurrency">The base currency for which to get the exchange rates.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The exchange rate response with the latest rates.</returns>
    public async Task<ExchangeRateResponse> GetExchangeRatesAsync(string baseCurrency, CancellationToken cancellationToken)
    {
        var requestUri = string.Format(Constants.CURRENCY_CONVERTER_URL_LATEST, baseCurrency);
        var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<ExchangeRateResponse>(content);
    }

    /// <summary>
    /// Retrieves historical exchange rates for a specified date range and base currency.
    /// </summary>
    /// <param name="query">The query containing the date range and base currency.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The exchange rate history response with the historical rates.</returns>
    public async Task<ExchangeRateHistoryResponse> GetHistoricRatesAsync(GetHistoricalRatesQuery query, CancellationToken cancellationToken)
    {
        var requestUri = string.Format(Constants.CURRENCY_CONVERTER_URL_HISTORIC_RATES, query.StartDate.ToString(Constants.DATEONLY_FORMAT), query.EndDate.ToString(Constants.DATEONLY_FORMAT), query.BaseCurrency);
        var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<ExchangeRateHistoryResponse>(content);
    }
}
