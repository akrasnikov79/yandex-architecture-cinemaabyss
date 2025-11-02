using CinemaAbyss.Events.Models;
using MassTransit;

namespace CinemaAbyss.Events.Consumers;

public class PaymentEventConsumer : IConsumer<PaymentEvent>
{
    private readonly ILogger<PaymentEventConsumer> _logger;

    public PaymentEventConsumer(ILogger<PaymentEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<PaymentEvent> context)
    {
        var paymentEvent = context.Message;
        
        _logger.LogInformation(
            "Received Payment Event: PaymentId={PaymentId}, UserId={UserId}, Amount={Amount}, Status={Status}, Timestamp={Timestamp}, MethodType={MethodType}, Partition={Partition}, Offset={Offset}",
            paymentEvent.PaymentId,
            paymentEvent.UserId,
            paymentEvent.Amount,
            paymentEvent.Status,
            paymentEvent.Timestamp,
            paymentEvent.MethodType ?? "N/A",
            context.ReceiveContext.TransportHeaders.Get<int>("kafka.partition", -1),
            context.ReceiveContext.TransportHeaders.Get<long>("kafka.offset", -1)
        );

        return Task.CompletedTask;
    }
}

