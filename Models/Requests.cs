using System.Text.Json.Serialization;

public class CreateGameRequest
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = string.Empty;
    
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = string.Empty;
}

public class JoinGameRequest
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = string.Empty;
    
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = string.Empty;
}
