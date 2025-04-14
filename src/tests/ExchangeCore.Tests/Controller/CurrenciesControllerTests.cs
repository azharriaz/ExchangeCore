using ExchangeCore.Api.Controllers;
using ExchangeCore.Api.Filters;
using ExchangeCore.Application.Common.Behaviours;
using ExchangeCore.Application.Common.Interfaces;
using ExchangeCore.Application.Common.Models;
using ExchangeCore.Application.CurrencyConverter.Queries.CurrencyConversion;
using ExchangeCore.Application.CurrencyConverter.Queries.GetLatestExchangeRates;
using ExchangeCore.Application.Dto;
using ExchangeCore.Domain.Interfaces;
using ExchangeCore.Domain.Models;
using ExchangeCore.Tests.Common;

using FluentValidation;

using Mapster;
using MapsterMapper;

using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ExchangeCore.Tests.Controller;
public class CurrenciesControllerTests
{
    private HttpClient _httpClient;
    private ServiceCollection _serviceCollection;
    private Mock<ICurrencyConverterService> _mockCurrencyConverterService = new();
    private Mock<IAppHybridCache> _mockHybridCache = new();

    private const string _testDataPath = "./TestData/currencies.json";

    public CurrenciesControllerTests()
    {
        _serviceCollection = new ServiceCollection();

        _serviceCollection.AddSingleton(GetConfiguredMappingConfig());
        _serviceCollection.AddScoped<IMapper, ServiceMapper>();

        _serviceCollection.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        _serviceCollection.AddMediatR(typeof(RetrieveLatestExchangeRatesHandler).Assembly);


        _serviceCollection.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        _serviceCollection.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));

        _serviceCollection.AddHttpContextAccessor();
    }

    #region GetLatestExchangeRates TestCases

    [Fact]
    public async Task GetLatestExchangeRates_ValidBaseCurrency_ReturnsOkResult()
    {
        // Arrange
        var requestUri = $"api/currencies/latest?baseCurrency={TestConstants.DEFAULT_BASE_CURRENCY}";
        var cancellationToken = new CancellationToken();

        LatestExchangeRatesConfigureServices(false);

        // Act
        var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        var latestExchangeRatesResponse = JsonSerializer.Deserialize<ServiceResult<ExchangeRatesDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        Assert.NotNull(latestExchangeRatesResponse);

        Assert.IsType<ExchangeRatesDto>(latestExchangeRatesResponse.Data);

        Assert.True(latestExchangeRatesResponse.Succeeded);
    }

    [Fact]
    public async Task GetLatestExchangeRates_InvalidBaseCurrency_ReturnsBadRequest()
    {
        // Arrange
        var requestUri = $"api/currencies/latest?baseCurrency=InvalidCurrency";
        var cancellationToken = new CancellationToken();

        LatestExchangeRatesConfigureServices(invalidDetails: true);

        // Act
        var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        var latestExchangeRatesResponse = JsonSerializer.Deserialize<ServiceResult<ExchangeRatesDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        Assert.NotNull(latestExchangeRatesResponse);

        Assert.False(latestExchangeRatesResponse.Succeeded);

        Assert.Null(latestExchangeRatesResponse.Data);
    }

    private void LatestExchangeRatesConfigureServices(bool invalidDetails)
    {
        var response = JsonHelper.GetRequestModelAsync<ExchangeRateResponse>(_testDataPath, ActionTypeEnum.Latest).Result;

        var mockExchangeRateResponse = new ExchangeRateResponse
        {
            Amount = 100.0,
            Base = "USD",
            Date = DateOnly.FromDateTime(DateTime.Now),
            Rates = new Dictionary<string, double>
            {
                { "EUR", 0.85 },
                { "GBP", 0.75 }
            }
        };

        // Setup GetOrCreateAsync to return the mockExchangeRateResponse when called
        _mockHybridCache.Setup(d => d.GetOrCreateAsync<ExchangeRateResponse>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<ExchangeRateResponse>>>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(invalidDetails ? default : mockExchangeRateResponse);

        _mockCurrencyConverterService
                .Setup(d => d.GetExchangeRatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(invalidDetails ? default : response);

        _serviceCollection.AddSingleton(_mockCurrencyConverterService.Object);
        _serviceCollection.AddSingleton(_mockHybridCache.Object);

        ConfigureServices();
    }
    #endregion

    #region ConvertCurrency TestCases

    [Fact]
    public async Task ConvertCurrency_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = await JsonHelper.GetRequestModelAsync<ConvertCurrencyQuery>(_testDataPath, ActionTypeEnum.Convert);
        var requestUri = $"api/currencies/convert";
        var cancellationToken = new CancellationToken();

        ConvertCurrencyConfigureServices(invalidDetails: false);

        // Act
        var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        var convertCurrencyResponse = JsonSerializer.Deserialize<ServiceResult<ExchangeRatesDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        Assert.NotNull(convertCurrencyResponse);

        Assert.IsType<ExchangeRatesDto>(convertCurrencyResponse.Data);

        Assert.True(convertCurrencyResponse.Succeeded);
    }

    [Fact]
    public async Task ConvertCurrency_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new ConvertCurrencyQuery() { FromCurrency = string.Empty, ToCurrency = string.Empty };
        var requestUri = "api/currencies/convert";
        var cancellationToken = new CancellationToken();

        ConvertCurrencyConfigureServices(invalidDetails: true);

        // Act
        var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        var convertCurrencyResponse = JsonSerializer.Deserialize<ServiceResult<ExchangeRatesDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        Assert.NotNull(convertCurrencyResponse);

        Assert.False(convertCurrencyResponse.Succeeded);

        Assert.Null(convertCurrencyResponse.Data);
    }

    private void ConvertCurrencyConfigureServices(bool invalidDetails)
    {
        var response = JsonHelper.GetRequestModelAsync<ExchangeRateResponse>(_testDataPath, ActionTypeEnum.ConvertResponse).Result;

        var mockExchangeRateResponse = new ExchangeRateResponse
        {
            Amount = 100.0,
            Base = "USD",
            Date = DateOnly.FromDateTime(DateTime.Now),
            Rates = new Dictionary<string, double>
            {
                { "EUR", 0.85 },
                { "GBP", 0.75 }
            }
        };

        // Setup GetOrCreateAsync to return the mockExchangeRateResponse when called
        _mockHybridCache.Setup(d => d.GetOrCreateAsync<ExchangeRateResponse>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<ExchangeRateResponse>>>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(invalidDetails ? default : mockExchangeRateResponse);

        _mockCurrencyConverterService
                .Setup(d => d.ConvertCurrencyAsync(It.IsAny<ConvertCurrencyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(invalidDetails ? default : response);

        _serviceCollection.AddSingleton(_mockCurrencyConverterService.Object);
        _serviceCollection.AddSingleton(_mockHybridCache.Object);

        ConfigureServices();
    }
    #endregion

    #region Common

    private void ConfigureServices(bool isAdminRole = false)
    {
        using var host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureTestServices(services =>
                    {

                        services
                            .AddAuthentication("TestScheme")
                            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

                        services.AddAuthorization(options =>
                        {
                            options.AddPolicy("AppUserRole", policy =>
                                policy.RequireRole("AppUser"));

                            options.AddPolicy("AdminUserRole", policy =>
                                policy.RequireRole("AdminUser"));
                        });

                        services.AddRouting();

                        foreach (var serviceDescriptor in _serviceCollection)
                        {
                            services.Add(serviceDescriptor);
                        }

                        var feature = new ControllerFeature();
                        var assembly = typeof(CurrenciesController).Assembly; // Replace YourController with your controller type
                        var manager = new ApplicationPartManager();
                        manager.ApplicationParts.Add(new AssemblyPart(assembly));
                        manager.FeatureProviders.Add(new ControllerFeatureProvider());
                        manager.PopulateFeature(feature);

                        services.AddSingleton(feature);
                        services.AddControllers(options =>
                            options.Filters.Add<ApiExceptionFilterAttribute>())
                        .ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(new AssemblyPart(assembly)));

                        // Customise default API behaviour
                        services.Configure<ApiBehaviorOptions>(options =>
                        {
                            options.SuppressModelStateInvalidFilter = true;
                        });
                    })
                    .Configure(app =>
                    {

                        app.UseRouting();

                        app.UseAuthentication();
                        app.UseAuthorization();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });
                    });
            })
            .StartAsync();

        _httpClient = host.Result.GetTestClient();
    }
    #endregion

    /// <summary>
    /// Mapster(Mapper) global configuration settings
    /// To learn more about Mapster,
    /// see https://github.com/MapsterMapper/Mapster
    /// </summary>
    /// <returns></returns>
    private static TypeAdapterConfig GetConfiguredMappingConfig()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        IList<IRegister> registers = config.Scan(Assembly.GetExecutingAssembly());

        config.Apply(registers);

        return config;
    }
}