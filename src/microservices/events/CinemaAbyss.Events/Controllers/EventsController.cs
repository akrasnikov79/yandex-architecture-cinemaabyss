using CinemaAbyss.Events.Models.Events;
using CinemaAbyss.Events.Models.Requests;
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
    public async Task<IActionResult> CreateMovie([FromBody] CreateMovieRequest request)
    {
        _logger.LogInformation(
            "Publishing movie event: MovieId={MovieId}, Title={Title}, Action={Action}",
            request.MovieId,
            request.Title,
            request.Action
        );

        var movieEvent = new MovieEvent
        {
            MovieId = request.MovieId,
            Title = request.Title,
            Action = request.Action,
            UserId = request.UserId,
            Rating = request.Rating,
            Genres = request.Genres,
            Description = request.Description
        };

        await _prodicer.Publish(movieEvent);
        return StatusCode(201, "movie-event-created");
    }

    [HttpPost("user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        _logger.LogInformation(
            "Publishing user event: UserId={UserId}, Action={Action}",
            request.UserId,
            request.Action
        );

        var userEvent = new UserEvent
        {
            UserId = request.UserId,
            Username = request.Username,
            Email = request.Email,
            Action = request.Action,
            Timestamp = DateTime.UtcNow
        };

        await _prodicer.Publish(userEvent);
        return StatusCode(201, "user-event-created");
    }

    [HttpPost("payment")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        _logger.LogInformation(
            "Publishing payment event: PaymentId={PaymentId}, UserId={UserId}, Amount={Amount}",
            request.PaymentId,
            request.UserId,
            request.Amount
        );

        var paymentEvent = new PaymentEvent
        {
            PaymentId = request.PaymentId,
            UserId = request.UserId,
            Amount = request.Amount,
            Status = request.Status,
            Timestamp = DateTime.UtcNow,
            MethodType = request.MethodType
        };

        await _prodicer.Publish(paymentEvent);
        return StatusCode(201, "payment-event-created");
    }
}

