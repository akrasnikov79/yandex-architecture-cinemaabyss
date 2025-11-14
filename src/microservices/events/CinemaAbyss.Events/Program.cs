using CinemaAbyss.Events.Consumers;
using CinemaAbyss.Events.Middleware;
using CinemaAbyss.Events.Models;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CinemaAbyss Events API", Version = "v1" });
});


var broker = builder.Configuration["Kafka:Brokers"] ?? throw new InvalidOperationException("kafka broker url is not configure");

builder.Services.AddMassTransit(x =>
{
    // Add consumers
    x.AddConsumer<MovieConsumer>();
    x.AddConsumer<UserConsumer>();
    x.AddConsumer<PaymentConsumer>();

    // Use InMemory for service bus (required even when using Kafka)
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });

    // Configure Kafka Rider
    x.AddRider(rider =>
    {
        // Configure Kafka producer for publishing events
        rider.AddProducer<MovieEvent>("movie-events");
        rider.AddProducer<UserEvent>("user-events");
        rider.AddProducer<PaymentEvent>("payment-events");

        // Configure Kafka consumers
        rider.AddConsumer<MovieConsumer>();
        rider.AddConsumer<UserConsumer>();
        rider.AddConsumer<PaymentConsumer>();

        rider.UsingKafka((context, k) =>
        {
            k.Host(broker);

            // Configure topic endpoints for consumers
            k.TopicEndpoint<MovieEvent>("movie-events", "events-service-group", e =>
            {
                e.ConfigureConsumer<MovieConsumer>(context);
                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
            });

            k.TopicEndpoint<UserEvent>("user-events", "events-service-group", e =>
            {
                e.ConfigureConsumer<UserConsumer>(context);
                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
            });

            k.TopicEndpoint<PaymentEvent>("payment-events", "events-service-group", e =>
            {
                e.ConfigureConsumer<PaymentConsumer>(context);
                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
            });
        });
    });
});

var app = builder.Build();

// Configure exception handler
app.UseExceptionHandler();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CinemaAbyss Events API v1"));
}

app.MapControllers();

var port = builder.Configuration["Port"] ?? "8082";
app.Run($"http://0.0.0.0:{port}");
