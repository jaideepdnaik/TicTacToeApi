public class GameState
{
    public required string GameId { get; set; }
    public required string[][] Board { get; set; }
    public required string CurrentPlayer { get; set; }
    public required string Winner { get; set; }
    public required bool IsGameOver { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime LastMoveAt { get; set; }
    public GameMode Mode { get; set; } = GameMode.SinglePlayer;
    public List<Player> Players { get; set; } = new();
    public string? CurrentPlayerTurn { get; set; }
    public bool IsDraw { get; set; }
    public bool IsWaitingForPlayer => Mode == GameMode.Multiplayer && Players.Count < 2;
}

public class Player
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty; // "X" or "O"
    public bool IsConnected { get; set; } = true;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public enum GameMode
{
    SinglePlayer,
    Multiplayer
}