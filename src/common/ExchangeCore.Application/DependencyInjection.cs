using ExchangeCore.Application.Common.Behaviours;
using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ExchangeCore.Application;

/// <summary>
/// adds application layer's services to dependencies container
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(GetConfiguredMappingConfig());
        services.AddScoped<IMapper, ServiceMapper>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));

        // Add FluentValidation services
        services.AddFluentValidationAutoValidation(); // Enables automatic validation
        services.AddFluentValidationClientsideAdapters(); // For client-side validation integration

        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });


        return services;
    }

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
