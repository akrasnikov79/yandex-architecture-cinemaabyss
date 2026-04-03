using CinemaAbyss.Events.Middleware;

namespace CinemaAbyss.Events.Extensions;

/// <summary>
/// Расширения для конфигурации middleware
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Добавляет сервисы middleware для обработки исключений и логирования
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <returns>Коллекция сервисов для цепочки вызовов</returns>
    public static IServiceCollection AddCustomMiddleware(this IServiceCollection services)
    {
        // Add middleware services
        services.AddScoped<RequestLoggerMiddleware>();

        // Add exception handling
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    /// <summary>
    /// Настраивает pipeline middleware для логирования и обработки исключений
    /// </summary>
    /// <param name="app">Экземпляр приложения</param>
    /// <returns>Экземпляр приложения для цепочки вызовов</returns>
    public static WebApplication UseCustomMiddleware(this WebApplication app)
    {
        // Configure request logger
        app.UseMiddleware<RequestLoggerMiddleware>();

        // Configure exception handler
        app.UseExceptionHandler();

        return app;
    }
}


