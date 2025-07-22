using System.Text.Json.Serialization;

public class Move
{
    [JsonPropertyName("row")]
    public int Row { get; set; }

    [JsonPropertyName("column")]
    public int Column { get; set; }

    [JsonPropertyName("player")]
    public required string Player { get; set; } // "X" or "O"
}