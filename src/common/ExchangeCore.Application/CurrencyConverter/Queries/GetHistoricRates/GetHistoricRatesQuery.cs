using ExchangeCore.Application.Common.Interfaces;
using ExchangeCore.Application.Common.Mapping;
using ExchangeCore.Application.Common.Models;
using ExchangeCore.Application.Dto;
using ExchangeCore.Domain.Interfaces;
using MapsterMapper;
using Microsoft.Extensions.Caching.Hybrid;

namespace ExchangeCore.Application.CurrencyConverter.Queries.GetHistoricRates;

public class GetHistoricalRatesQuery : IRequestWrapper<PaginatedGetHistoricRatesDto>
{
    public string BaseCurrency { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetHistoricalRatesHandler(IMapper mapper,
    ICurrencyConverterService currencyConverterService, IAppHybridCache cache)
    : IRequestHandlerWrapper<GetHistoricalRatesQuery, PaginatedGetHistoricRatesDto>
{
    public async Task<ServiceResult<PaginatedGetHistoricRatesDto>> Handle(GetHistoricalRatesQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = $"exchange-rate:histrates:{query.BaseCurrency}:{query.StartDate:yyyyMMdd}:{query.EndDate:yyyyMMdd}";

        var cacheOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(15),
        };

        var cachedHistoricRates = await cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                return await currencyConverterService.GetHistoricRatesAsync(query, cancellationToken).ConfigureAwait(false);
            },
            cacheOptions,
            cancellationToken: cancellationToken
        );

        if (cachedHistoricRates == default)
        {
            return ServiceResult.Failed<PaginatedGetHistoricRatesDto>(ServiceError.NotFound);
        }

        // Map exchangeRates to GetHistoricRatesDto using Mapster
        var historicRatesDto = mapper.Map<GetHistoricRatesDto>(cachedHistoricRates);

        // Paginate the Rates dictionary
        var paginatedRates = await cachedHistoricRates.Rates
            .PaginatedListAsync(query.PageNumber, query.PageSize);

        // Create a new instance of GetHistoricRatesDto with paginated Rates
        var paginatedHistoricRatesDto = new GetHistoricRatesDto
        {
            Amount = historicRatesDto.Amount,
            Base = historicRatesDto.Base,
            StartDate = historicRatesDto.StartDate,
            EndDate = historicRatesDto.EndDate,
            Rates = paginatedRates.Items.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value
            )
        };

        // Create a new instance of PaginatedGetHistoricRatesDto
        var paginatedDto = new PaginatedGetHistoricRatesDto
        {
            Data = paginatedHistoricRatesDto,
            PageIndex = paginatedRates.PageIndex,
            TotalPages = paginatedRates.TotalPages,
            TotalCount = paginatedRates.TotalCount
        };

        return ServiceResult.Success(paginatedDto);
    }
}
