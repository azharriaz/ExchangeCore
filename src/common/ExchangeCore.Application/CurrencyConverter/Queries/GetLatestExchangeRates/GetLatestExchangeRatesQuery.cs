using ExchangeCore.Application.Common.Interfaces;
using ExchangeCore.Application.Common.Models;
using ExchangeCore.Application.Dto;
using ExchangeCore.Domain.Interfaces;
using MapsterMapper;
using Microsoft.Extensions.Caching.Hybrid;

namespace ExchangeCore.Application.CurrencyConverter.Queries.GetLatestExchangeRates;

public class GetLatestExchangeRatesQuery : IRequestWrapper<ExchangeRatesDto>
{
    public required string BaseCurrency { get; set; }
}

public class RetrieveLatestExchangeRatesHandler(IMapper mapper, ICurrencyConverterService currencyConverterService, IAppHybridCache cache) 
    : IRequestHandlerWrapper<GetLatestExchangeRatesQuery, ExchangeRatesDto>
{
    public async Task<ServiceResult<ExchangeRatesDto>> Handle(GetLatestExchangeRatesQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = $"exchange-rate:latest:{query.BaseCurrency?.ToUpperInvariant()}";

        var cacheOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(15),
        };

        var result = await cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                var result = await currencyConverterService.GetExchangeRatesAsync(query.BaseCurrency, cancellationToken)
                .ConfigureAwait(false);

                // Only cache if the request succeeded and data is valid
                if (result is not null)
                {
                    return result;
                }

                return null; // exception can also be thrown if required.
            },
            cacheOptions,
            cancellationToken: cancellationToken
            );

        return result == default
            ? ServiceResult.Failed<ExchangeRatesDto>(ServiceError.NotFound)
            : ServiceResult.Success(mapper.Map<ExchangeRatesDto>(result));
    }
}
