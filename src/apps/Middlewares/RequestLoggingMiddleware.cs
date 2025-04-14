using ExchangeCore.Domain.Common;
using System.Diagnostics;

namespace ExchangeCore.API.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);

            var clientId = context.User.Claims.FirstOrDefault(c => c.Type == Constants.JWT_Client_Id_Key)?.Value ?? "anonymous";

            _logger.LogInformation("Request: {Method} {Path} from {ClientIp} (ClientId: {ClientId}) responded {StatusCode} in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress?.ToString(),
                clientId,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request: {Method} {Path} from {ClientIp} failed after {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress?.ToString(),
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}

/// <summary>
/// configures middleware to use it from pipeline configurations
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}