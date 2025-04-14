using FluentValidation;

namespace ExchangeCore.Application.CurrencyConverter.Queries.GetHistoricRates;

public class GetHistoricRatesValidator : AbstractValidator<GetHistoricalRatesQuery>
{
    public GetHistoricRatesValidator()
    {
        RuleFor(x => x.BaseCurrency)
                .NotNull()
                .NotEmpty().WithMessage("Base Currency is required.");

        RuleFor(x => x.StartDate)
                .NotNull()
                .NotEmpty().WithMessage("Start Date is required.");

        RuleFor(x => x.EndDate)
                .NotNull()
                .NotEmpty().WithMessage("End Date is required.");

        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(query => query.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.");
    }
}
