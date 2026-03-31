namespace CinemaAbyss.Events.Extensions;

/// <summary>
/// Расширения для конфигурации Swagger/OpenAPI
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Добавляет сервисы Swagger/OpenAPI для генерации документации API
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="title">Название API</param>
    /// <param name="version">Версия API</param>
    /// <returns>Коллекция сервисов для цепочки вызовов</returns>
    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services,
        string title = "CinemaAbyss Events API",
        string version = "v1")
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(version, new() { Title = title, Version = version });
        });

        return services;
    }

    /// <summary>
    /// Настраивает middleware для Swagger UI (только для Development окружения)
    /// </summary>
    /// <param name="app">Экземпляр приложения</param>
    /// <param name="version">Версия API</param>
    /// <returns>Экземпляр приложения для цепочки вызовов</returns>
    public static WebApplication UseSwaggerDocumentation(
        this WebApplication app,
        string version = "v1")
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"CinemaAbyss Events API {version}"));
        }

        return app;
    }
}


