using System.Text.Json;
using CinemaAbyss.Events.Confluent.Models;
using Confluent.Kafka;

namespace CinemaAbyss.Events.Confluent.Services;

public class KafkaProducerService : IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
    {
        _logger = logger;
        
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:Brokers"] ?? "localhost:9092",
            ClientId = "events-service-producer"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task<(int Partition, long Offset)> ProduceMovieEventAsync(MovieEvent movieEvent)
    {
        var topic = "movie-events";
        var key = movieEvent.MovieId.ToString();
        var value = JsonSerializer.Serialize(movieEvent);

        try
        {
            var result = await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = key,
                Value = value
            });

            _logger.LogInformation(
                "Produced movie event to {Topic}: Partition={Partition}, Offset={Offset}",
                topic, result.Partition.Value, result.Offset.Value
            );

            return (result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Error producing movie event to Kafka");
            throw;
        }
    }

    public async Task<(int Partition, long Offset)> ProduceUserEventAsync(UserEvent userEvent)
    {
        var topic = "user-events";
        var key = userEvent.UserId.ToString();
        var value = JsonSerializer.Serialize(userEvent);

        try
        {
            var result = await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = key,
                Value = value
            });

            _logger.LogInformation(
                "Produced user event to {Topic}: Partition={Partition}, Offset={Offset}",
                topic, result.Partition.Value, result.Offset.Value
            );

            return (result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Error producing user event to Kafka");
            throw;
        }
    }

    public async Task<(int Partition, long Offset)> ProducePaymentEventAsync(PaymentEvent paymentEvent)
    {
        var topic = "payment-events";
        var key = paymentEvent.PaymentId.ToString();
        var value = JsonSerializer.Serialize(paymentEvent);

        try
        {
            var result = await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = key,
                Value = value
            });

            _logger.LogInformation(
                "Produced payment event to {Topic}: Partition={Partition}, Offset={Offset}",
                topic, result.Partition.Value, result.Offset.Value
            );

            return (result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Error producing payment event to Kafka");
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}


