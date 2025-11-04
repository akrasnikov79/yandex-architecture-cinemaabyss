using Microsoft.OpenApi.Models;
using MfbProxy.OcelotGateway;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Values;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MfbProxy Ocelot Gateway",
        Version = "v1",
        Description = "API endpoints exposed by the Ocelot gateway.",
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
        // получаем функцию для доступа к списку сервисов, которые определены в ocelot.json
        Func<Task<List<Service>>> servicesAccessor = discoveryProvider.GetAsync;
        var logger = serviceProvider.GetRequiredService<ILogger<PercentageLoadBalancer>>();
        return new PercentageLoadBalancer(servicesAccessor, logger);
    });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MfbProxy Ocelot Gateway v1");
});

app.MapGet("/", () => Results.Ok(new
{
    Service = "MfbProxy Ocelot Gateway",
    Timestamp = DateTime.UtcNow
}))
.WithName("GatewayHealth")
.WithSummary("Returns gateway basic information.")
.WithDescription("Health endpoint confirming that the Ocelot gateway is running.");

await app.UseOcelot();
await app.RunAsync();
