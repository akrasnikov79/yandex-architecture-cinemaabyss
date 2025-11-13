using CinemaAbyss.Events.Models;
using System.Net;
using System.Text.Json;

namespace CinemaAbyss.Events.Middleware;

/// <summary>
/// Middleware для глобальной обработки необработанных исключений
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment environment)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}",
                context.TraceIdentifier,
                context.Request.Path
            );

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var errorResponse = HandleException(exception, context);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = errorResponse.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private ErrorResponse HandleException(Exception exception, HttpContext context)
    {
        var errorResponse = exception switch
        {
            ArgumentNullException argNullEx => CreateErrorResponse(
                HttpStatusCode.BadRequest,
                "Bad Request",
                $"Required parameter is missing: {argNullEx.ParamName}",
                context.TraceIdentifier
            ),

            ArgumentException argEx => CreateErrorResponse(
                HttpStatusCode.BadRequest,
                "Bad Request",
                argEx.Message,
                context.TraceIdentifier
            ),

            UnauthorizedAccessException => CreateErrorResponse(
                HttpStatusCode.Unauthorized,
                "Unauthorized",
                "You are not authorized to access this resource",
                context.TraceIdentifier
            ),

            InvalidOperationException invalidOpEx => CreateErrorResponse(
                HttpStatusCode.BadRequest,
                "Invalid Operation",
                invalidOpEx.Message,
                context.TraceIdentifier
            ),

            _ => CreateErrorResponse(
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                GetErrorMessage(exception),
                context.TraceIdentifier
            )
        };

        return errorResponse;
    }

    private static ErrorResponse CreateErrorResponse(HttpStatusCode statusCode, string error, string message, string traceId) => new ErrorResponse
    {
        StatusCode = (int)statusCode,
        Error = error,
        Message = message,
        TraceId = traceId
    };

    private string GetErrorMessage(Exception exception)
    {
        // В Production не показываем детали исключений
        if (_environment.IsProduction())
        {
            return "An error occurred while processing your request";
        }

        // В Development показываем полную информацию
        return exception.Message;
    }
}
