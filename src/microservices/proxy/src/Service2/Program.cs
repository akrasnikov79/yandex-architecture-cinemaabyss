var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Service = "Service2",
    Port = 5002,
    Timestamp = DateTime.UtcNow
}))
.WithName("HealthCheck");

// Weather forecast endpoint
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    
    return new
    {
        Service = "Service2",
        Port = 5002,
        Timestamp = DateTime.UtcNow,
        Data = forecast
    };
})
.WithName("GetWeatherForecast");

// Test endpoint for load balancing
app.MapGet("/api/test", () => Results.Ok(new
{
    Message = "Response from Service2",
    Service = "Service2",
    Port = 5002,
    Timestamp = DateTime.UtcNow,
    MachineName = Environment.MachineName
}))
.WithName("Test");

Console.WriteLine("🚀 Service2 started on https://localhost:5002");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
