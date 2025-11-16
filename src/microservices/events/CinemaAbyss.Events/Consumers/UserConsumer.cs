using CinemaAbyss.Events.Models.Events;
using MassTransit;

namespace CinemaAbyss.Events.Consumers;

public class UserConsumer(ILogger<UserConsumer> logger) : IConsumer<UserEvent>
{
    private readonly ILogger<UserConsumer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task Consume(ConsumeContext<UserEvent> context)
    {
        var userEvent = context.Message;
        
        _logger.LogInformation(
            "Received User Event: UserId={UserId}, Username={Username}, Email={Email}, Action={Action}, Timestamp={Timestamp}, Partition={Partition}, Offset={Offset}",
            userEvent.UserId,
            userEvent.Username ?? "N/A",
            userEvent.Email ?? "N/A",
            userEvent.Action,
            userEvent.Timestamp,
            context.ReceiveContext.TransportHeaders.Get<int>("kafka.partition", -1),
            context.ReceiveContext.TransportHeaders.Get<long>("kafka.offset", -1)
        );

        return Task.CompletedTask;
    }
}

