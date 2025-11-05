using MfbProxy.OcelotGateway.Midllewares;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Values;

var builder = WebApplication.CreateBuilder(args);

const string configurationsDirectory = "Configurations";
var env = builder.Environment;
 

builder.Configuration
    .AddJsonFile($"{configurationsDirectory}/appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"{configurationsDirectory}/appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"{configurationsDirectory}/ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"{configurationsDirectory}/ocelot.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddMoviesMigrationHealthChecks();

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

app.MapMoviesMigrationHealthChecks();

await app.UseOcelot();
await app.RunAsync();
