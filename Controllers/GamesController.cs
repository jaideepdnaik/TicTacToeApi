using Microsoft.AspNetCore.Mvc;

namespace TicTacToeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(IGameService gameService, ILogger<GamesController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    [HttpPost]
    public ActionResult<GameState> CreateGame()
    {
        try
        {
            var gameState = _gameService.CreateGame();
            _logger.LogInformation("New game created with ID: {GameId}", gameState.GameId);
            return CreatedAtAction(nameof(GetGame), new { gameId = gameState.GameId }, gameState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new game");
            return StatusCode(500, "An error occurred while creating the game");
        }
    }

    [HttpGet("multiplayer/available")]
    public ActionResult<List<GameState>> GetAvailableMultiplayerGames()
    {
        try
        {
            var games = _gameService.GetAvailableMultiplayerGames();
            _logger.LogInformation("Retrieved {GameCount} available multiplayer games", games.Count);
            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available multiplayer games");
            return StatusCode(500, "An error occurred while retrieving available games");
        }
    }

    [HttpPost("multiplayer")]
    public ActionResult<GameState> CreateMultiplayerGame([FromBody] CreateGameRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PlayerName))
            {
                return BadRequest("Player name cannot be empty");
            }

            var gameState = _gameService.CreateMultiplayerGame(request.PlayerId, request.PlayerName);
            _logger.LogInformation("New multiplayer game created with ID: {GameId}", gameState.GameId);
            return CreatedAtAction(nameof(GetGame), new { gameId = gameState.GameId }, gameState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new multiplayer game");
            return StatusCode(500, "An error occurred while creating the game");
        }
    }

    [HttpPost("{gameId}/join")]
    public ActionResult JoinGame(string gameId, [FromBody] JoinGameRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                return BadRequest("Game ID cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(request.PlayerName))
            {
                return BadRequest("Player name cannot be empty");
            }

            var success = _gameService.JoinMultiplayerGame(gameId, request.PlayerId, request.PlayerName);
            if (!success)
            {
                return BadRequest("Cannot join game");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining game {GameId}", gameId);
            return StatusCode(500, "An error occurred while joining the game");
        }
    }

    [HttpGet]
    public ActionResult<List<GameState>> GetAllGames()
    {
        try
        {
            var games = _gameService.GetAllGames();
            _logger.LogInformation("Retrieved {GameCount} games", games.Count);
            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all games");
            return StatusCode(500, "An error occurred while retrieving games");
        }
    }

    [HttpGet("{gameId}")]
    public ActionResult<GameState> GetGame(string gameId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                return BadRequest("Game ID cannot be empty");
            }

            var gameState = _gameService.GetGame(gameId);
            if (gameState == null)
            {
                _logger.LogWarning("Game with ID {GameId} not found", gameId);
                return NotFound($"Game with ID {gameId} not found");
            }

            return Ok(gameState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game with ID: {GameId}", gameId);
            return StatusCode(500, "An error occurred while retrieving the game");
        }
    }


    [HttpPost("{gameId}/moves")]
    public ActionResult<GameResponse> MakeMove(string gameId, [FromBody] Move move)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                return BadRequest("Game ID cannot be empty");
            }

            if (move == null)
            {
                return BadRequest("Move cannot be null");
            }

            if (string.IsNullOrWhiteSpace(move.Player))
            {
                return BadRequest("Player cannot be empty");
            }

            if (move.Player != "X" && move.Player != "O")
            {
                return BadRequest("Player must be 'X' or 'O'");
            }

            var response = _gameService.MakeMove(gameId, move);
            
            if (!response.Success)
            {
                _logger.LogWarning("Invalid move attempted in game {GameId}: {Message}", gameId, response.Message);
                return BadRequest(response);
            }

            _logger.LogInformation("Move made successfully in game {GameId} by player {Player} at ({Row}, {Column})", 
                gameId, move.Player, move.Row, move.Column);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making move in game {GameId}", gameId);
            return StatusCode(500, "An error occurred while making the move");
        }
    }

    [HttpDelete("{gameId}")]
    public ActionResult DeleteGame(string gameId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                return BadRequest("Game ID cannot be empty");
            }

            var deleted = _gameService.DeleteGame(gameId);
            if (!deleted)
            {
                _logger.LogWarning("Attempted to delete non-existent game: {GameId}", gameId);
                return NotFound($"Game with ID {gameId} not found");
            }

            _logger.LogInformation("Game {GameId} deleted successfully", gameId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting game with ID: {GameId}", gameId);
            return StatusCode(500, "An error occurred while deleting the game");
        }
    }
}
