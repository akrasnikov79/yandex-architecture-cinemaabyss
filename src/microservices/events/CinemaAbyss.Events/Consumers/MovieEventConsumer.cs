using CinemaAbyss.Events.Models;
using MassTransit;

namespace CinemaAbyss.Events.Consumers;

public class MovieEventConsumer : IConsumer<MovieEvent>
{
    private readonly ILogger<MovieEventConsumer> _logger;

    public MovieEventConsumer(ILogger<MovieEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<MovieEvent> context)
    {
        var movieEvent = context.Message;
        
        _logger.LogInformation(
            "Received Movie Event: MovieId={MovieId}, Title={Title}, Action={Action}, UserId={UserId}, Partition={Partition}, Offset={Offset}",
            movieEvent.MovieId,
            movieEvent.Title,
            movieEvent.Action,
            movieEvent.UserId,
            context.ReceiveContext.TransportHeaders.Get<int>("kafka.partition", -1),
            context.ReceiveContext.TransportHeaders.Get<long>("kafka.offset", -1)
        );

        _logger.LogInformation(
            "Movie Event Details - Rating={Rating}, Genres={Genres}, Description={Description}",
            movieEvent.Rating,
            movieEvent.Genres != null ? string.Join(", ", movieEvent.Genres) : "N/A",
            movieEvent.Description ?? "N/A"
        );

        return Task.CompletedTask;
    }
}

