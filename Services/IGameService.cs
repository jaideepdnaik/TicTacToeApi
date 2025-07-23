public interface IGameService
{
    GameState CreateGame();
    GameState CreateMultiplayerGame(string playerId, string playerName);
    GameState? GetGame(string gameId);
    List<GameState> GetAllGames();
    List<GameState> GetAvailableMultiplayerGames();
    GameResponse MakeMove(string gameId, Move move);
    bool DeleteGame(string gameId);
    bool JoinMultiplayerGame(string gameId, string playerId, string playerName);
    bool LeaveGame(string gameId, string playerId);
    void HandlePlayerDisconnection(string playerId);
}