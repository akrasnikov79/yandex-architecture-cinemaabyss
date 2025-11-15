using CinemaAbyss.Events.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add custom middleware
builder.Services.AddCustomMiddleware();

// Add Swagger/OpenAPI
builder.Services.AddSwaggerDocumentation();

// Add message broker (Kafka with MassTransit)
builder.Services.AddMessageBroker(builder.Configuration);

var app = builder.Build();

// Configure custom middleware
app.UseCustomMiddleware();

// Configure Swagger documentation
app.UseSwaggerDocumentation();

app.MapControllers();

var port = builder.Configuration["Port"] ?? "8082";
app.Run($"http://0.0.0.0:{port}");
