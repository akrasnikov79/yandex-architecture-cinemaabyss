using CinemaAbyss.Events.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace CinemaAbyss.Events.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IPublishEndpoint _prodicer;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IPublishEndpoint publishEndpoint, ILogger<EventsController> logger)
    {
        _prodicer = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("movie")]
    public async Task<IActionResult> CreateMovie([FromBody] MovieEvent movieEvent)
    {
        try
        {
            _logger.LogInformation(
                "Publishing movie event: MovieId={MovieId}, Title={Title}, Action={Action}",
                movieEvent.MovieId,
                movieEvent.Title,
                movieEvent.Action
            );

            await _prodicer.Publish(movieEvent);  
            return StatusCode(201, "movie-event-created");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing movie event");
            return StatusCode(500, new { error = "Internal Server Error" });
        }
    }

    [HttpPost("user")]
    public async Task<IActionResult> CreateUser([FromBody] UserEvent userEvent)
    {
        try
        {
            _logger.LogInformation(
                "Publishing user event: UserId={UserId}, Action={Action}",
                userEvent.UserId,
                userEvent.Action
            );

            await _prodicer.Publish(userEvent); 
            return StatusCode(201, "user-event-created");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing user event");
            return StatusCode(500, new { error = "Internal Server Error" });
        }
    }

    [HttpPost("payment")]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentEvent paymentEvent)
    {
        try
        {
            _logger.LogInformation(
                "Publishing payment event: PaymentId={PaymentId}, UserId={UserId}, Amount={Amount}",
                paymentEvent.PaymentId,
                paymentEvent.UserId,
                paymentEvent.Amount
            );

            await _prodicer.Publish(paymentEvent); 
            return StatusCode(201, "payment-event-created");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing payment event");
            return StatusCode(500, new { error = "Internal Server Error" });
        }
    }
}

