using ExchangeCore.Application.CurrencyConverter.Queries.CurrencyConversion;
using ExchangeCore.Application.CurrencyConverter.Queries.GetHistoricRates;
using ExchangeCore.Domain.Models;

using System.Threading;
using System.Threading.Tasks;

namespace ExchangeCore.Application.Common.Interfaces;

public interface ICurrencyConverterService
{
    Task<ExchangeRateResponse> GetExchangeRatesAsync(string baseCurrency, CancellationToken cancellationToken);

    Task<ExchangeRateResponse> ConvertCurrencyAsync(ConvertCurrencyQuery command, CancellationToken cancellationToken);
    
    Task<ExchangeRateHistoryResponse> GetHistoricRatesAsync(GetHistoricalRatesQuery query, CancellationToken cancellationToken);
}
