using CinemaAbyss.Events.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace CinemaAbyss.Events.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        IPublishEndpoint publishEndpoint,
        ILogger<EventsController> logger)
    {
        _publishEndpoint = publishEndpoint;
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
                "Publishing movie event: MovieId={MovieId}, Title={Title}, Action={Action}",
                movieEvent.MovieId,
                movieEvent.Title,
                movieEvent.Action
            );

            await _publishEndpoint.Publish(movieEvent);

            var eventResponse = new EventResponse
            {
                Status = "success",
                Partition = 0, // MassTransit handles partitioning internally
                Offset = 0,    // Kafka will assign actual offset
                Event = new EventData
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
            _logger.LogError(ex, "Error publishing movie event");
            return StatusCode(500, new { error = "Internal Server Error" });
        }
    }

    [HttpPost("user")]
    public async Task<IActionResult> CreateUserEvent([FromBody] UserEvent userEvent)
    {
        try
        {
            _logger.LogInformation(
                "Publishing user event: UserId={UserId}, Action={Action}",
                userEvent.UserId,
                userEvent.Action
            );

            await _publishEndpoint.Publish(userEvent);

            var eventResponse = new EventResponse
            {
                Status = "success",
                Partition = 0,
                Offset = 0,
                Event = new EventData
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
            _logger.LogError(ex, "Error publishing user event");
            return StatusCode(500, new { error = "Internal Server Error" });
        }
    }

    [HttpPost("payment")]
    public async Task<IActionResult> CreatePaymentEvent([FromBody] PaymentEvent paymentEvent)
    {
        try
        {
            _logger.LogInformation(
                "Publishing payment event: PaymentId={PaymentId}, UserId={UserId}, Amount={Amount}",
                paymentEvent.PaymentId,
                paymentEvent.UserId,
                paymentEvent.Amount
            );

            await _publishEndpoint.Publish(paymentEvent);

            var eventResponse = new EventResponse
            {
                Status = "success",
                Partition = 0,
                Offset = 0,
                Event = new EventData
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
            _logger.LogError(ex, "Error publishing payment event");
            return StatusCode(500, new { error = "Internal Server Error" });
        }
    }
}

