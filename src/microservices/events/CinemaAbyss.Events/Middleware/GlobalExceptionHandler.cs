using CinemaAbyss.Events.Models;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace CinemaAbyss.Events.Middleware;

/// <summary>
/// Глобальный обработчик исключений, реализующий IExceptionHandler
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHostEnvironment _environment = environment ?? throw new ArgumentNullException(nameof(environment));

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}",
            httpContext.TraceIdentifier,
            httpContext.Request.Path
        );

        var (statusCode, error, message) = exception switch
        {
            ArgumentNullException arg => (
                HttpStatusCode.BadRequest,
                "Bad Request",
                $"Required parameter is missing: {arg.ParamName}"
            ),

            ArgumentException arg => (
                HttpStatusCode.BadRequest,
                "Bad Request",
                arg.Message
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                GetSafeErrorMessage(exception)
            )
        };

        var response = new ErrorResponse
        {
            Error = error,
            Message = message,
            StatusCode = (int)statusCode,
            TraceId = httpContext.TraceIdentifier
        };

        httpContext.Response.StatusCode = (int)statusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private string GetSafeErrorMessage(Exception exception) =>
        _environment.IsProduction()
            ? "An error occurred while processing your request"
            : exception.Message;
}

