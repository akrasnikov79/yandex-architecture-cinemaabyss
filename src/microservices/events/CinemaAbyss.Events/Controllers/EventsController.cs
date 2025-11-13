using CinemaAbyss.Events.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace CinemaAbyss.Events.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(IPublishEndpoint publishEndpoint, ILogger<EventsController> logger) : ControllerBase
{
    private readonly IPublishEndpoint _prodicer = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    private readonly ILogger<EventsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpPost("movie")]
    public async Task<IActionResult> CreateMovie([FromBody] MovieEvent movieEvent)
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

    [HttpPost("user")]
    public async Task<IActionResult> CreateUser([FromBody] UserEvent userEvent)
    {
        _logger.LogInformation(
            "Publishing user event: UserId={UserId}, Action={Action}",
            userEvent.UserId,
            userEvent.Action
        );

        await _prodicer.Publish(userEvent);
        return StatusCode(201, "user-event-created");
    }

    [HttpPost("payment")]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentEvent paymentEvent)
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
}

