using System.Text.Json;
using CinemaAbyss.Events.Confluent.Models;
using Confluent.Kafka;

namespace CinemaAbyss.Events.Confluent.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IConfiguration _configuration;

    public KafkaConsumerService(IConfiguration configuration, ILogger<KafkaConsumerService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:Brokers"] ?? "localhost:9092",
            GroupId = "events-service-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        
        consumer.Subscribe(new[] { "movie-events", "user-events", "payment-events" });
        _logger.LogInformation("Kafka Consumer started. Subscribed to topics: movie-events, user-events, payment-events");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    if (consumeResult?.Message != null)
                    {
                        ProcessMessage(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Kafka Consumer");
        }
        finally
        {
            consumer.Close();
            _logger.LogInformation("Kafka Consumer stopped");
        }

        return Task.CompletedTask;
    }

    private void ProcessMessage(ConsumeResult<string, string> result)
    {
        var topic = result.Topic;
        var partition = result.Partition.Value;
        var offset = result.Offset.Value;
        var key = result.Message.Key;
        var value = result.Message.Value;

        _logger.LogInformation(
            "Consumed message from {Topic}: Partition={Partition}, Offset={Offset}, Key={Key}",
            topic, partition, offset, key
        );

        try
        {
            switch (topic)
            {
                case "movie-events":
                    var movieEvent = JsonSerializer.Deserialize<MovieEvent>(value);
                    if (movieEvent != null)
                    {
                        _logger.LogInformation(
                            "Movie Event Details - MovieId={MovieId}, Title={Title}, Action={Action}, UserId={UserId}, Rating={Rating}",
                            movieEvent.MovieId, movieEvent.Title, movieEvent.Action, movieEvent.UserId, movieEvent.Rating
                        );
                    }
                    break;

                case "user-events":
                    var userEvent = JsonSerializer.Deserialize<UserEvent>(value);
                    if (userEvent != null)
                    {
                        _logger.LogInformation(
                            "User Event Details - UserId={UserId}, Username={Username}, Email={Email}, Action={Action}, Timestamp={Timestamp}",
                            userEvent.UserId, userEvent.Username, userEvent.Email, userEvent.Action, userEvent.Timestamp
                        );
                    }
                    break;

                case "payment-events":
                    var paymentEvent = JsonSerializer.Deserialize<PaymentEvent>(value);
                    if (paymentEvent != null)
                    {
                        _logger.LogInformation(
                            "Payment Event Details - PaymentId={PaymentId}, UserId={UserId}, Amount={Amount}, Status={Status}, Timestamp={Timestamp}, MethodType={MethodType}",
                            paymentEvent.PaymentId, paymentEvent.UserId, paymentEvent.Amount, paymentEvent.Status, paymentEvent.Timestamp, paymentEvent.MethodType
                        );
                    }
                    break;

                default:
                    _logger.LogWarning("Received message from unknown topic: {Topic}", topic);
                    break;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing message from {Topic}", topic);
        }
    }
}

