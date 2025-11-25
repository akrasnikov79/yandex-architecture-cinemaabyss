using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CinemaAbyss.Events.Models.Requests;

/// <summary>
/// Модель запроса для создания события о платеже
/// </summary>
public class CreatePaymentRequest
{
    [JsonPropertyName("payment_id")]
    [Required(ErrorMessage = "Payment ID is required")]
    public int PaymentId { get; set; }

    [JsonPropertyName("user_id")]
    [Required(ErrorMessage = "User ID is required")]
    public int UserId { get; set; }

    [JsonPropertyName("amount")]
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [JsonPropertyName("status")]
    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status must not exceed 50 characters")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("method_type")]
    [StringLength(50, ErrorMessage = "Method type must not exceed 50 characters")]
    public string? MethodType { get; set; }
}



