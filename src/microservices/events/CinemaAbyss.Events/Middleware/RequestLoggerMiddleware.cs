using System.Diagnostics;

namespace CinemaAbyss.Events.Middleware;

/// <summary>
/// Middleware для логирования HTTP запросов и ответов
/// </summary>
public class RequestLoggerMiddleware : IMiddleware
{
    private readonly ILogger<RequestLoggerMiddleware> _logger;

    public RequestLoggerMiddleware(ILogger<RequestLoggerMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var traceId = context.TraceIdentifier;
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var requestQueryString = context.Request.QueryString.HasValue 
            ? context.Request.QueryString.Value 
            : string.Empty;

        _logger.LogInformation(
            "Incoming request: {Method} {Path}{QueryString} | TraceId: {TraceId}",
            requestMethod,
            requestPath,
            requestQueryString,
            traceId
        );

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            var logLevel = GetLogLevel(statusCode);

            _logger.Log(
                logLevel,
                "Completed request: {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms | TraceId: {TraceId}",
                requestMethod,
                requestPath,
                statusCode,
                elapsedMilliseconds,
                traceId
            );
        }
    }

    private static LogLevel GetLogLevel(int statusCode) => statusCode switch
    {
        >= 500 => LogLevel.Error,
        >= 400 => LogLevel.Warning,
        _ => LogLevel.Information
    };
}

