using ExchangeCore.Application.Common.Interfaces;
using ExchangeCore.Application.Common.Models;
using ExchangeCore.Application.Dto;
using ExchangeCore.Domain.Interfaces;
using MapsterMapper;
using Microsoft.Extensions.Caching.Hybrid;

namespace ExchangeCore.Application.CurrencyConverter.Queries.CurrencyConversion;

public class ConvertCurrencyQuery : IRequestWrapper<ConvertCurrencyDto>
{
    public double Amount { get; set; }
    public required string FromCurrency { get; set; }
    public required string ToCurrency { get; set; }
}

public class ConvertCurrencyHandler(IMapper mapper, ICurrencyConverterService currencyConverterService, IAppHybridCache cache)
    : IRequestHandlerWrapper<ConvertCurrencyQuery, ConvertCurrencyDto>
{
    public async Task<ServiceResult<ConvertCurrencyDto>> Handle(ConvertCurrencyQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = $"exchange-rate:convert:{query.FromCurrency}:{query.ToCurrency}:{query.Amount}";

        var cacheOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(5), // Shorter TTL than exchange rates (conversions are dynamic)
        };

        var result = await cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                return await currencyConverterService.ConvertCurrencyAsync(query, cancellationToken).ConfigureAwait(false);
            },
            cacheOptions,
            cancellationToken: cancellationToken
        );

        return result == default
            ? ServiceResult.Failed<ConvertCurrencyDto>(ServiceError.NotFound)
            : ServiceResult.Success(mapper.Map<ConvertCurrencyDto>(result));
    }
}
