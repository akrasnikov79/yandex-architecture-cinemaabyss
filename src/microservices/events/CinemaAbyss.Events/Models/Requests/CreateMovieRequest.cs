using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CinemaAbyss.Events.Models.Requests;

/// <summary>
/// Модель запроса для создания события о фильме
/// </summary>
public class CreateMovieRequest
{
    [JsonPropertyName("movie_id")]
    [Required(ErrorMessage = "Movie ID is required")]
    public int MovieId { get; set; }

    [JsonPropertyName("title")]
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("action")]
    [Required(ErrorMessage = "Action is required")]
    [StringLength(50, ErrorMessage = "Action must not exceed 50 characters")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public int? UserId { get; set; }

    [JsonPropertyName("rating")]
    [Range(0, 10, ErrorMessage = "Rating must be between 0 and 10")]
    public double? Rating { get; set; }

    [JsonPropertyName("genres")]
    public List<string>? Genres { get; set; }

    [JsonPropertyName("description")]
    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
    public string? Description { get; set; }
}

