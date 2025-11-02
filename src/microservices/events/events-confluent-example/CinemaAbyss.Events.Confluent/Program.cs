using CinemaAbyss.Events.Confluent.Services;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CinemaAbyss Events API (Confluent.Kafka)", Version = "v1" });
});

// Register Kafka services
builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CinemaAbyss Events API (Confluent.Kafka) v1"));
}

app.MapControllers();

var port = builder.Configuration["Port"] ?? "8083";
app.Run($"http://0.0.0.0:{port}");
