using FluentValidation;
using System.Text.RegularExpressions;

namespace ExchangeCore.Application.Auth.Commands;

public class GetTokenQueryValidator : AbstractValidator<LoginCommand>
{
    public GetTokenQueryValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters.")
            .Matches(@"^[a-zA-Z0-9_.-]*$").WithMessage("Username contains invalid characters.")
            .Must(NotContainSqlInjectionPatterns).WithMessage("Username contains unsafe characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.")
            .Must(NotContainSqlInjectionPatterns).WithMessage("Password contains unsafe characters.");
    }

    private bool NotContainSqlInjectionPatterns(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;

        var blacklistPatterns = new[]
        {
            @"--", @"\*", @"\b(SELECT|INSERT|DELETE|DROP|UPDATE|EXEC|UNION|ALTER)\b", @"[';]", @"\bOR\b", @"\bAND\b"
        };

        foreach (var pattern in blacklistPatterns)
        {
            if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                return false;
        }

        return true;
    }
}

