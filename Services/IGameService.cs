public interface IGameService
{
    GameState CreateGame();
    GameState? GetGame(string gameId);
    GameResponse MakeMove(string gameId, Move move);
    bool DeleteGame(string gameId);
    List<GameState> GetAllGames();
}