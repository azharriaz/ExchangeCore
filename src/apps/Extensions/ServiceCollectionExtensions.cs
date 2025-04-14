using ExchangeCore.Api.Filters;
using ExchangeCore.Application;
using ExchangeCore.Infrastructure;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace ExchangeCore.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder ConfigureAppServices(this WebApplicationBuilder builder)
    {
        var logger = Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.With(new OpenTelemetryEnricher())  // Add our custom enricher
            .WriteTo.Console(outputTemplate:
                "[{TraceId}] [{SpanId}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3} {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        builder.Host.UseSerilog();

        // Single OpenTelemetry configuration
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: builder.Environment.ApplicationName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.EnrichWithHttpRequest = (activity, httpRequest) =>
                    {
                        activity.SetTag("client.ip", httpRequest.HttpContext.Connection.RemoteIpAddress?.ToString());

                        // This should now work because JWT authentication is set up first
                        var clientId = httpRequest.HttpContext.User.FindFirst("client_id")?.Value;
                        if (!string.IsNullOrEmpty(clientId))
                        {
                            activity.SetTag("client.id", clientId);
                        }
                    };
                    options.EnrichWithHttpResponse = (activity, httpResponse) =>
                    {
                        activity.SetTag("http.response_code", httpResponse.StatusCode);
                    };
                    options.RecordException = true;
                })
                .AddHttpClientInstrumentation()
                .AddConsoleExporter()
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri("http://localhost:4317");
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddConsoleExporter()
                .AddOtlpExporter());


        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Exchange Core API",
                Version = "v1"
            });
        });

        // Add services to the container.

        builder.Services.AddApplication(builder.Configuration);
        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.AddControllers(options =>
                options.Filters.Add<ApiExceptionFilterAttribute>());

        return builder;
    }
}

public class OpenTelemetryEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("TraceId", activity.TraceId));

            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("SpanId", activity.SpanId));

            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("ParentId", activity.ParentSpanId));
        }
    }
}