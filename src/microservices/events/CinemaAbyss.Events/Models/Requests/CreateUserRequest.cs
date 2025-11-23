using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CinemaAbyss.Events.Models.Requests;

/// <summary>
/// Модель запроса для создания события о пользователе
/// </summary>
public class CreateUserRequest
{
    [JsonPropertyName("user_id")]
    [Required(ErrorMessage = "User ID is required")]
    public int UserId { get; set; }

    [JsonPropertyName("username")]
    [StringLength(100, ErrorMessage = "Username must not exceed 100 characters")]
    public string? Username { get; set; }

    [JsonPropertyName("email")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [JsonPropertyName("action")]
    [Required(ErrorMessage = "Action is required")]
    [StringLength(50, ErrorMessage = "Action must not exceed 50 characters")]
    public string Action { get; set; } = string.Empty;
}


