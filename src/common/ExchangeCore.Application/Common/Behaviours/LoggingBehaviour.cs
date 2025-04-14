using ExchangeCore.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestName = typeof(TRequest).Name;
        var httpContext = _httpContextAccessor.HttpContext;

        try
        {
            var response = await next();
            stopwatch.Stop();

            // Log successful completion
            var clientId = httpContext?.User.FindFirst(Constants.JWT_Client_Id_Key)?.Value ?? "anonymous";

            _logger.LogInformation(
                "Request Completed: {RequestName} {RequestPath} (ClientId: {ClientId}) " +
                "Status: {StatusCode} in {ElapsedMs}ms",
                requestName,
                httpContext?.Request.Path,
                clientId,
                httpContext?.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "Request Failed: {RequestName} {RequestPath} from {ClientIp} " +
                "after {ElapsedMs}ms - Error: {ErrorMessage}",
                requestName,
                httpContext?.Request.Path,
                httpContext?.Connection.RemoteIpAddress?.ToString(),
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            throw;
        }
    }
}
