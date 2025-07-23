using Microsoft.AspNetCore.SignalR;

namespace TicTacToeApi.Hubs;

public class GameHub : Hub
{
    private readonly IGameService _gameService;
    private readonly ILogger<GameHub> _logger;

    public GameHub(IGameService gameService, ILogger<GameHub> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    public async Task JoinGame(string gameId, string playerName)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
            
            var success = _gameService.JoinMultiplayerGame(gameId, Context.ConnectionId, playerName);
            if (success)
            {
                await Clients.OthersInGroup(gameId).SendAsync("PlayerJoined", new { 
                    PlayerId = Context.ConnectionId, 
                    PlayerName = playerName 
                });

                var gameState = _gameService.GetGame(gameId);
                await Clients.Caller.SendAsync("GameJoined", gameState);
                
                _logger.LogInformation("Player {PlayerName} joined game {GameId}", playerName, gameId);
            }
            else
            {
                await Clients.Caller.SendAsync("JoinGameFailed", "Cannot join this game");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining game {GameId}", gameId);
            await Clients.Caller.SendAsync("Error", "Failed to join game");
        }
    }

    public async Task CreateGame(string playerName)
    {
        try
        {
            var gameState = _gameService.CreateMultiplayerGame(Context.ConnectionId, playerName);
            await Groups.AddToGroupAsync(Context.ConnectionId, gameState.GameId);
            
            await Clients.Caller.SendAsync("GameCreated", gameState);
            _logger.LogInformation("New multiplayer game created with ID: {GameId} by {PlayerName}", gameState.GameId, playerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game");
            await Clients.Caller.SendAsync("Error", "Failed to create game");
        }
    }

    public async Task MakeMove(string gameId, Move move)
    {
        try
        {
            move.Player = Context.ConnectionId; // Use connection ID as player identifier
            var response = _gameService.MakeMove(gameId, move);
            
            if (response.Success)
            {
                // Broadcast the move to all players in the game
                await Clients.Group(gameId).SendAsync("MoveMade", new {
                    GameId = gameId,
                    Move = move,
                    GameState = response.Game,
                    PlayerId = Context.ConnectionId
                });

                if (response.Game?.Winner != null || response.Game?.IsDraw == true)
                {
                    await Clients.Group(gameId).SendAsync("GameEnded", new {
                        Winner = response.Game.Winner,
                        IsDraw = response.Game.IsDraw,
                        GameState = response.Game
                    });
                }
            }
            else
            {
                await Clients.Caller.SendAsync("MoveRejected", response.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making move in game {GameId}", gameId);
            await Clients.Caller.SendAsync("Error", "Failed to make move");
        }
    }

    public async Task LeaveGame(string gameId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
            _gameService.LeaveGame(gameId, Context.ConnectionId);
            
            await Clients.OthersInGroup(gameId).SendAsync("PlayerLeft", new {
                PlayerId = Context.ConnectionId
            });
            
            _logger.LogInformation("Player {PlayerId} left game {GameId}", Context.ConnectionId, gameId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving game {GameId}", gameId);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            // Handle player disconnection - remove from all games
            _gameService.HandlePlayerDisconnection(Context.ConnectionId);
            
            // Notify other players about disconnection
            await Clients.All.SendAsync("PlayerDisconnected", Context.ConnectionId);
            
            _logger.LogInformation("Player {PlayerId} disconnected", Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling disconnection for player {PlayerId}", Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
