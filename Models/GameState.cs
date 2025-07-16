public class GameState
{
    public required string GameId { get; set; }
    public required string[,] Board { get; set; }
    public required string CurrentPlayer { get; set; }
    public required string Winner { get; set; }
    public required bool IsGameOver { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime LastMoveAt { get; set; }
}