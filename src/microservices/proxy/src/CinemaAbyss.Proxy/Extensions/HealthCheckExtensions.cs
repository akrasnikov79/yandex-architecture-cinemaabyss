using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

static class HealthCheckExtensions
{
    public static IServiceCollection AddMoviesMigrationHealthChecks(this IServiceCollection services)
    {
        return services.AddHealthChecks()
            .AddCheck("self", () => 
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Gateway is running"))
            .AddCheck("migration_config", () =>
            {
                var migrationPercent = Environment.GetEnvironmentVariable("MOVIES_MIGRATION_PERCENT");
                if (string.IsNullOrWhiteSpace(migrationPercent))
                {
                    return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded("MOVIES_MIGRATION_PERCENT not configured, using default");
                }
                
                if (int.TryParse(migrationPercent, out var percent) && percent >= 0 && percent <= 100)
                {
                    return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy($"Migration percent: {percent}%");
                }
                
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy($"Invalid MOVIES_MIGRATION_PERCENT value: {migrationPercent}");
            })
            .Services;
    }

    public static WebApplication UseMoviesMigrationHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(x => new
                    {
                        name = x.Key,
                        status = x.Value.Status.ToString(),
                        description = x.Value.Description,
                        duration = x.Value.Duration.TotalMilliseconds
                    }),
                    totalDuration = report.TotalDuration.TotalMilliseconds,
                    timestamp = DateTime.UtcNow
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }));
            }
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = _ => true, // Выполнить все проверки для готовности
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync(report.Status.ToString());
            }
        });

        return app;
    }
}
