using System.Text.Json.Serialization;

namespace CinemaAbyss.Events.Models;

public class PaymentEvent
{
    [JsonPropertyName("payment_id")]
    public int PaymentId { get; set; }

    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("method_type")]
    public string? MethodType { get; set; }
}

