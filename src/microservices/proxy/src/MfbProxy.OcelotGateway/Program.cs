using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MfbProxy.OcelotGateway;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Values;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddHealthChecks()
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
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services
    .AddOcelot(builder.Configuration)
    .AddCustomLoadBalancer((serviceProvider, route, discoveryProvider) =>
    {
        if (discoveryProvider is null)
        {
            throw new InvalidOperationException("Service discovery provider is not configured for the current route.");
        }
        
        Func<Task<List<Service>>> services = discoveryProvider.GetAsync;
        var logger = serviceProvider.GetRequiredService<ILogger<PercentageLoadBalancer>>();
        return new PercentageLoadBalancer(services, logger);
    });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();

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
 
await app.UseOcelot();
await app.RunAsync();
