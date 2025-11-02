using CinemaAbyss.Events.Models;
using MassTransit;

namespace CinemaAbyss.Events.Consumers;

public class UserEventConsumer : IConsumer<UserEvent>
{
    private readonly ILogger<UserEventConsumer> _logger;

    public UserEventConsumer(ILogger<UserEventConsumer> logger)
    {
        _logger = logger;
    }

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

