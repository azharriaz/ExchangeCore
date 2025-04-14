using ExchangeCore.Domain.Models;

using Mapster;

namespace ExchangeCore.Application.Dto;

public class GetHistoricRatesDto : IRegister
{
    public double Amount { get; set; }
    public string Base { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public Dictionary<DateOnly, Dictionary<string, double>> Rates { get; set; }

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ExchangeRateResponse, GetHistoricRatesDto>();
    }
}

public class PaginatedGetHistoricRatesDto
{
    public GetHistoricRatesDto Data { get; set; }
    public int PageIndex { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}
