public class GameState
{
    public string GameId { get; set; }
    public string[,] Board { get; set; }
    public string CurrentPlayer { get; set; }
    public string Winner { get; set; }
    public bool IsGameOver { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastMoveAt { get; set; }
}