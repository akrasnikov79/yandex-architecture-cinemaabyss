using CinemaAbyss.Events.Consumers;
using CinemaAbyss.Events.Models;
using MassTransit;

namespace CinemaAbyss.Events.Extensions;

/// <summary>
/// Расширения для конфигурации брокера сообщений
/// </summary>
public static class BrokerExtensions
{
    /// <summary>
    /// Добавляет и настраивает MassTransit с Kafka для обработки событий
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="configuration">Конфигурация приложения</param>
    /// <returns>Коллекция сервисов для цепочки вызовов</returns>
    public static IServiceCollection AddMessageBroker(this IServiceCollection services, IConfiguration configuration)
    {
        var broker = configuration["Kafka:Brokers"] 
            ?? throw new InvalidOperationException("kafka broker url is not configure");

        services.AddMassTransit(x =>
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

        return services;
    }
}

