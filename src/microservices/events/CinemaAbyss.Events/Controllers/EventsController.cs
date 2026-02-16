using CinemaAbyss.Events.Models;
using CinemaAbyss.Events.Models.Events;
using CinemaAbyss.Events.Models.Requests;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace CinemaAbyss.Events.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(ILogger<EventsController> logger) : ControllerBase
{
    private readonly ILogger<EventsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = true });
    }

    [HttpPost("movie")]
    public async Task<IActionResult> CreateMovie(
        [FromBody] CreateMovieRequest request,
        [FromServices] ITopicProducer<MovieEvent> movieProducer)
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

        await movieProducer.Produce(movieEvent);
        return StatusCode(201, "movie-event-created");
    }

    [HttpPost("user")]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest request,
        [FromServices] ITopicProducer<UserEvent> userProducer)
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

        await userProducer.Produce(userEvent);
        return StatusCode(201, "user-event-created");
    }

    [HttpPost("payment")]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        [FromServices] ITopicProducer<PaymentEvent> paymentProducer)
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

        await paymentProducer.Produce(paymentEvent);
        return StatusCode(201, "payment-event-created");
    }
}

