using ExchangeCore.Application.Common.Interfaces;
using ExchangeCore.Domain.Common;
using ExchangeCore.Domain.ConfigOptions.CurrencyConverter;
using ExchangeCore.Domain.Interfaces;
using ExchangeCore.Infrastructure.Data;
using ExchangeCore.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Text;

namespace ExchangeCore.Infrastructure;

/// <summary>
/// adds/configures infrastructure services to dependencies container
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CurrencyConverterOptions>(configuration.GetSection(CurrencyConverterOptions.SectionName));

        var currencyConverterOptions = configuration.GetSection(CurrencyConverterOptions.SectionName).Get<CurrencyConverterOptions>();

        ConfigureHttpClient(services, currencyConverterOptions);

        services.AddHybridCache(options =>
        {
            // Maximum size of cached items
            options.MaximumPayloadBytes = 1024 * 1024 * 10; // 10MB
            options.MaximumKeyLength = 512;

            // Default timeouts
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(10)
            };
        });

        services.AddTransient<IDateTime, DateTimeService>();
        services.AddTransient<ICurrencyConverterService, CurrencyConverterService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddSingleton<IAppHybridCache, AppHybridCache>();

        services.AddSingleton<IUserStore<IdentityUser>, InMemoryUserStore>();
        services.AddSingleton<IRoleStore<IdentityRole>, InMemoryRoleStore>();

        services.AddIdentityCore<IdentityUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
        })
        .AddRoles<IdentityRole>()
        .AddDefaultTokenProviders();

        // Configure authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
            };
        });

        // Configure authorization
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AppUserRole", policy => policy.RequireRole("appUser"));
            options.AddPolicy("AdminUserRole", policy => policy.RequireRole("adminUser"));
        });

        return services;
    }

    /// <summary>
    /// Configures a policy registry with a retry policy configured for transient HTTP errors.
    /// </summary>
    /// <param name="retries">The maximum number of retry attempts.</param>
    /// <param name="initialDelaySeconds">The initial delay in seconds before the first retry attempt. The delay will increase exponentially with each retry.</param>
    /// <returns>A PolicyRegistry containing the configured retry policy.</returns>
    private static void ConfigureHttpClient(IServiceCollection services, CurrencyConverterOptions options)
    {
        services.AddHttpClient(Constants.CURRENCY_CONVERTER_CLIENT, client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
        })
        .AddResilienceHandler("retry-circuit", builder =>
        {
            // Configure Retry
            builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = args => args.Outcome switch
                {
                    { Exception: HttpRequestException } => PredicateResult.True(),
                    { Result: HttpResponseMessage response } when !response.IsSuccessStatusCode => PredicateResult.True(),
                    _ => PredicateResult.False()
                },
                MaxRetryAttempts = options.RetryPolicy.MaxRetries,
                Delay = TimeSpan.FromSeconds(options.RetryPolicy.RetryIntervalSeconds),
                OnRetry = args =>
                {
                    Console.WriteLine($"Retry attempt {args.AttemptNumber} due to: {args.Outcome.Exception?.Message}");
                    return default;
                }
            });

            // Configure Circuit Breaker
            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = args => args.Outcome switch
                {
                    { Exception: HttpRequestException } => PredicateResult.True(),
                    { Result: HttpResponseMessage response } when !response.IsSuccessStatusCode => PredicateResult.True(),
                    _ => PredicateResult.False()
                },
                FailureRatio = 0.3, // Break if 30% of requests fail
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = options.CircuitBreakerPolicy.MinimumRequestsThrouput, // Minimum requests before evaluating
                BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerPolicy.CircuitBreakerDurationSeconds),
                OnOpened = args =>
                {
                    Console.WriteLine($"Circuit opened! Break duration: {args.BreakDuration.TotalSeconds}s");
                    return default;
                },
                OnClosed = _ =>
                {
                    Console.WriteLine("Circuit closed.");
                    return default;
                },
                OnHalfOpened = _ =>
                {
                    Console.WriteLine("Circuit half-opened (testing).");
                    return default;
                }
            });
        });
    }
}
