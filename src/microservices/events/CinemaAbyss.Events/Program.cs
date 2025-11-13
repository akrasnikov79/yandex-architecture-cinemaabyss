using CinemaAbyss.Events.Consumers;
using CinemaAbyss.Events.Models;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CinemaAbyss Events API", Version = "v1" });
});

// Configure MassTransit with Kafka
//var kafkaBrokers = builder.Configuration["Kafka:Brokers"] ?? "localhost:9092";

//builder.Services.AddMassTransit(x =>
//{
//    // Add consumers
//    x.AddConsumer<MovieEventConsumer>();
//    x.AddConsumer<UserEventConsumer>();
//    x.AddConsumer<PaymentEventConsumer>();

//    // Use InMemory for service bus (required even when using Kafka)
//    x.UsingInMemory((context, cfg) =>
//    {
//        cfg.ConfigureEndpoints(context);
//    });

//    // Configure Kafka Rider
//    x.AddRider(rider =>
//    {
//        // Configure Kafka producer for publishing events
//        rider.AddProducer<MovieEvent>("movie-events");
//        rider.AddProducer<UserEvent>("user-events");
//        rider.AddProducer<PaymentEvent>("payment-events");

//        // Configure Kafka consumers
//        rider.AddConsumer<MovieEventConsumer>();
//        rider.AddConsumer<UserEventConsumer>();
//        rider.AddConsumer<PaymentEventConsumer>();

//        rider.UsingKafka((context, k) =>
//        {
//            k.Host(kafkaBrokers);

//            // Configure topic endpoints for consumers
//            k.TopicEndpoint<MovieEvent>("movie-events", "events-service-group", e =>
//            {
//                e.ConfigureConsumer<MovieEventConsumer>(context);
//                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
//            });

//            k.TopicEndpoint<UserEvent>("user-events", "events-service-group", e =>
//            {
//                e.ConfigureConsumer<UserEventConsumer>(context);
//                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
//            });

//            k.TopicEndpoint<PaymentEvent>("payment-events", "events-service-group", e =>
//            {
//                e.ConfigureConsumer<PaymentEventConsumer>(context);
//                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
//            });
//        });
//    });
//});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CinemaAbyss Events API v1"));
}

app.MapControllers();

var port = builder.Configuration["Port"] ?? "8082";
app.Run($"http://0.0.0.0:{port}");
