public class GameResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public GameState? Game { get; set; }
}