public class GameService : IGameService
{
    private readonly Dictionary<string, GameState> _games = [];

    public GameState CreateGame()
    {
        var gameId = Guid.NewGuid().ToString();

        var gameState = new GameState
        {
            GameId = gameId,
            Board = new string[3][]
            {
                new string[3],
                new string[3],
                new string[3]
            },
            CurrentPlayer = "X",
            Winner = string.Empty,
            IsGameOver = false,
            CreatedAt = DateTime.UtcNow,
            LastMoveAt = DateTime.UtcNow,
            Mode = GameMode.SinglePlayer
        };

        _games[gameId] = gameState;

        return gameState;
    }

    public GameState CreateMultiplayerGame(string playerId, string playerName)
    {
        var gameState = new GameState
        {
            GameId = Guid.NewGuid().ToString(),
            Board =
            [
                new string[3],
                new string[3],
                new string[3]
            ],
            CurrentPlayer = "X",
            Winner = string.Empty,
            IsGameOver = false,
            CreatedAt = DateTime.UtcNow,
            LastMoveAt = DateTime.UtcNow,
            Mode = GameMode.Multiplayer
        };

        var player = new Player
        {
            Id = playerId,
            Name = playerName,
            Symbol = "X",
            IsConnected = true
        };

        gameState.Players.Add(player);
        gameState.CurrentPlayerTurn = playerId;

        _games[gameState.GameId] = gameState;
        return gameState;
    }

    public bool JoinMultiplayerGame(string gameId, string playerId, string playerName)
    {
        if (!_games.TryGetValue(gameId, out var gameState))
            return false;

        if (gameState.Mode != GameMode.Multiplayer)
            return false;

        if (gameState.Players.Count >= 2)
            return false;

        if (gameState.Players.Any(p => p.Id == playerId))
            return true; // Already in game

        var player = new Player
        {
            Id = playerId,
            Name = playerName,
            Symbol = "O", // Second player gets O
            IsConnected = true
        };

        gameState.Players.Add(player);
        gameState.LastMoveAt = DateTime.UtcNow;

        return true;
    }

    public bool LeaveGame(string gameId, string playerId)
    {
        if (!_games.TryGetValue(gameId, out var gameState))
            return false;

        var player = gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
            return false;

        player.IsConnected = false;
        return true;
    }

    public void HandlePlayerDisconnection(string playerId)
    {
        foreach (var game in _games.Values)
        {
            var player = game.Players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                player.IsConnected = false;
            }
        }
    }

    public List<GameState> GetAvailableMultiplayerGames()
    {
        return _games.Values
            .Where(g => g.Mode == GameMode.Multiplayer && 
                       g.IsWaitingForPlayer && 
                       !g.IsGameOver)
            .ToList();
    }

    public GameState? GetGame(string gameId)
    {
        _games.TryGetValue(gameId, out var gameState);
        return gameState;
    }

    public GameResponse MakeMove(string gameId, Move move)
    {
        if (!_games.TryGetValue(gameId, out var gameState) || gameState.IsGameOver)
        {
            return new GameResponse
            {
                Success = false,
                Message = "Game not found or already over."
            };
        }

        // For multiplayer games, validate turn
        if (gameState.Mode == GameMode.Multiplayer)
        {
            if (gameState.CurrentPlayerTurn != move.Player)
            {
                return new GameResponse
                {
                    Success = false,
                    Message = "Not your turn"
                };
            }

            var player = gameState.Players.FirstOrDefault(p => p.Id == move.Player);
            if (player == null)
            {
                return new GameResponse
                {
                    Success = false,
                    Message = "Player not in this game"
                };
            }

            // Use player's symbol instead of move.Player
            var playerSymbol = player.Symbol;
            
            // Use IsValidMove helper method for comprehensive validation
            if (!IsValidMove(gameState.Board, move))
            {
                return new GameResponse
                {
                    Success = false,
                    Message = "Invalid move. Cell is occupied or coordinates are out of bounds."
                };
            }

            gameState.Board[move.Row][move.Column] = playerSymbol;
            gameState.LastMoveAt = DateTime.UtcNow;

            if (CheckForWinner(gameState, move.Row, move.Column))
            {
                gameState.Winner = playerSymbol;
                gameState.IsGameOver = true;
            }
            else if (IsBoardFull(gameState.Board))
            {
                gameState.Winner = "Draw";
                gameState.IsGameOver = true;
                gameState.IsDraw = true;
            }
            else
            {
                // Update turn for multiplayer
                var otherPlayer = gameState.Players.FirstOrDefault(p => p.Id != gameState.CurrentPlayerTurn);
                gameState.CurrentPlayerTurn = otherPlayer?.Id;
                gameState.CurrentPlayer = otherPlayer?.Symbol ?? gameState.CurrentPlayer;
            }
        }
        else
        {
            // Single player game logic (existing)
            // Use IsValidMove helper method for comprehensive validation
            if (!IsValidMove(gameState.Board, move))
            {
                return new GameResponse
                {
                    Success = false,
                    Message = "Invalid move. Cell is occupied or coordinates are out of bounds."
                };
            }

            // Additional validation: Check if it's the correct player's turn
            if (move.Player != gameState.CurrentPlayer)
            {
                return new GameResponse
                {
                    Success = false,
                    Message = $"It's player {gameState.CurrentPlayer}'s turn."
                };
            }

            gameState.Board[move.Row][move.Column] = gameState.CurrentPlayer;
            gameState.LastMoveAt = DateTime.UtcNow;

            if (CheckForWinner(gameState, move.Row, move.Column))
            {
                gameState.Winner = gameState.CurrentPlayer;
                gameState.IsGameOver = true;
            }
            else if (IsBoardFull(gameState.Board))
            {
                gameState.Winner = "Draw";
                gameState.IsGameOver = true;
                gameState.IsDraw = true;
            }
            else
            {
                gameState.CurrentPlayer = GetNextPlayer(gameState);
            }
        }

        return new GameResponse
        {
            Success = true,
            Message = "Move made successfully.",
            Game = gameState,
            GameState = gameState
        };
    }

    private bool IsValidMove(string[][] board, Move move)
    {
        return move.Row >= 0 && move.Row < 3 && move.Column >= 0 && move.Column < 3 && string.IsNullOrEmpty(board[move.Row][move.Column]);
    }

    private bool CheckForWinner(GameState gameState, int row, int column)
    {
        var player = gameState.Board[row][column];
        if (string.IsNullOrEmpty(player)) return false;

        // Check row
        if (gameState.Board[row][0] == player && gameState.Board[row][1] == player && gameState.Board[row][2] == player)
            return true;

        // Check column
        if (gameState.Board[0][column] == player && gameState.Board[1][column] == player && gameState.Board[2][column] == player)
            return true;

        // Check diagonals
        if (row == column)
        {
            if (gameState.Board[0][0] == player && gameState.Board[1][1] == player && gameState.Board[2][2] == player)
                return true;
        }

        if (row + column == 2)
        {
            if (gameState.Board[0][2] == player && gameState.Board[1][1] == player && gameState.Board[2][0] == player)
                return true;
        }

        return false;
    }

    private bool IsBoardFull(string[][] board)
    {
        return !board.SelectMany(row => row).Any(string.IsNullOrEmpty);
    }

    private string GetNextPlayer(GameState gameState)
    {
        return gameState.CurrentPlayer == "X" ? "O" : "X";
    }

    public bool DeleteGame(string gameId)
    {
        return _games.Remove(gameId);
    }

    public List<GameState> GetAllGames()
    {
        return [.. _games.Values];
    }
}