using CinemaAbyss.Events.Confluent.Models;
using CinemaAbyss.Events.Confluent.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaAbyss.Events.Confluent.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly KafkaProducerService _producerService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        KafkaProducerService producerService,
        ILogger<EventsController> logger)
    {
        _producerService = producerService;
        _logger = logger;
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = true });
    }

    [HttpPost("movie")]
    public async Task<IActionResult> CreateMovieEvent([FromBody] MovieEvent movieEvent)
    {
        try
        {
            _logger.LogInformation(
                "Producing movie event: MovieId={MovieId}, Title={Title}, Action={Action}",
                movieEvent.MovieId,
                movieEvent.Title,
                movieEvent.Action
            );

            var (partition, offset) = await _producerService.ProduceMovieEventAsync(movieEvent);

            var eventResponse = new EventResponse
            {
                Status = "success",
                Partition = partition,
                Offset = offset,
                Event = new Event
                {
                    Id = $"movie-{movieEvent.MovieId}-{movieEvent.Action}",
                    Type = "movie",
                    Timestamp = DateTime.UtcNow,
                    Payload = movieEvent
                }
            };

            return StatusCode(201, eventResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error producing movie event");
            return StatusCode(500, new { error = "Internal Server Error" });
        }
    }

    [HttpPost("user")]
    public async Task<IActionResult> CreateUserEvent([FromBody] UserEvent userEvent)
    {
        try
        {
            _logger.LogInformation(
                "Producing user event: UserId={UserId}, Action={Action}",
                userEvent.UserId,
                userEvent.Action
            );

            var (partition, offset) = await _producerService.ProduceUserEventAsync(userEvent);

            var eventResponse = new EventResponse
            {
                Status = "success",
                Partition = partition,
                Offset = offset,
                Event = new Event
                {
                    Id = $"user-{userEvent.UserId}-{userEvent.Action}",
                    Type = "user",
                    Timestamp = userEvent.Timestamp,
                    Payload = userEvent
                }
            };

            return StatusCode(201, eventResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error producing user event");
            return StatusCode(500, new { error = "Internal Server Error" });
        }
    }

    [HttpPost("payment")]
    public async Task<IActionResult> CreatePaymentEvent([FromBody] PaymentEvent paymentEvent)
    {
        try
        {
            _logger.LogInformation(
                "Producing payment event: PaymentId={PaymentId}, UserId={UserId}, Amount={Amount}",
                paymentEvent.PaymentId,
                paymentEvent.UserId,
                paymentEvent.Amount
            );

            var (partition, offset) = await _producerService.ProducePaymentEventAsync(paymentEvent);

            var eventResponse = new EventResponse
            {
                Status = "success",
                Partition = partition,
                Offset = offset,
                Event = new Event
                {
                    Id = $"payment-{paymentEvent.PaymentId}",
                    Type = "payment",
                    Timestamp = paymentEvent.Timestamp,
                    Payload = paymentEvent
                }
            };

            return StatusCode(201, eventResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error producing payment event");
            return StatusCode(500, new { error = "Internal Server Error" });
        }
    }
}

