using System.Text.Json.Serialization;

namespace CinemaAbyss.Events.Confluent.Models;

public class Event
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("payload")]
    public object Payload { get; set; } = new();
}


