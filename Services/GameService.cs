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
            LastMoveAt = DateTime.UtcNow
        };

        _games[gameId] = gameState;

        return gameState;
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
        }
        else
        {
            gameState.CurrentPlayer = GetNextPlayer(gameState);
        }

        return new GameResponse
        {
            Success = true,
            Message = "Move made successfully.",
            Game = gameState
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