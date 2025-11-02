using System.Text.Json.Serialization;

namespace CinemaAbyss.Events.Confluent.Models;

public class EventResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("partition")]
    public int Partition { get; set; }

    [JsonPropertyName("offset")]
    public long Offset { get; set; }

    [JsonPropertyName("event")]
    public Event Event { get; set; } = new();
}

