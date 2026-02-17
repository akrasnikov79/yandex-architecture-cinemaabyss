using CinemaAbyss.Proxy.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

const string configurationsDirectory = "Configurations";
var env = builder.Environment;


builder.Configuration
    .AddJsonFile($"{configurationsDirectory}/appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"{configurationsDirectory}/appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"{configurationsDirectory}/ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"{configurationsDirectory}/ocelot.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

//builder.Services.AddMoviesMigrationHealthChecks();

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
    .AddPercentageBalancer();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();

//app.UseMoviesMigrationHealthChecks();

app.MapGet("/health", () => Results.Ok(new { status = true }));

await app.UseOcelot();

if (app.Environment.IsDevelopment())
{
    await app.RunAsync();
    return;
}

var port = Environment.GetEnvironmentVariable("PORT") ?? throw new ArgumentNullException("port is null");
await app.RunAsync($"http://0.0.0.0:{port}");
