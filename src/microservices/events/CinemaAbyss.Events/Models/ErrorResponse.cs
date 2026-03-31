namespace CinemaAbyss.Events.Models;

/// <summary>
/// Модель ответа об ошибке
/// </summary>
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? TraceId { get; set; }
}

