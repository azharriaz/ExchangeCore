using ExchangeCore.Application.CurrencyConverter.Queries.GetLatestExchangeRates;

using FluentValidation;

namespace ExchangeCore.Application.CurrencyConverter.Queries.GetLatestExchangeRate;

public class GetLatestExchangeRatesValidator : AbstractValidator<GetLatestExchangeRatesQuery>
{
    public GetLatestExchangeRatesValidator()
    {
        RuleFor(x => x.BaseCurrency)
                .NotNull()
                .NotEmpty().WithMessage("Base Currency is required.");
    }
}
