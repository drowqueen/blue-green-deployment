using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSignalR();
builder.Services.AddSingleton<GameStateService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure middleware
app.UseCors("AllowAll");
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/health", async context =>
    {
        await context.Response.WriteAsJsonAsync(new { status = "healthy", timestamp = DateTime.UtcNow });
    });

    endpoints.MapGet("/leaderboard", async context =>
    {
        var gameState = context.RequestServices.GetRequiredService<GameStateService>();
        var leaderboard = gameState.GetLeaderboard();
        await context.Response.WriteAsJsonAsync(leaderboard);
    });

    endpoints.MapGet("/game-state", async context =>
    {
        var gameState = context.RequestServices.GetRequiredService<GameStateService>();
        var state = gameState.GetGameState();
        await context.Response.WriteAsJsonAsync(state);
    });

    endpoints.MapHub<GameHub>("/ws");
});

app.Run();

public class GameHub : Hub
{
    private readonly GameStateService _gameState;

    public GameHub(GameStateService gameState)
    {
        _gameState = gameState;
    }

    public override async Task OnConnectedAsync()
    {
        var playerId = Context.ConnectionId;
        _gameState.AddPlayer(playerId);
        await Clients.Caller.SendAsync("ReceiveMessage", new { action = "join", status = "success", playerId });
        await BroadcastGameState();
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _gameState.RemovePlayer(Context.ConnectionId);
        await BroadcastGameState();
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendPlayerUpdate(string message)
    {
        var data = JsonSerializer.Deserialize<PlayerUpdate>(message);
        if (data?.Action == "move" && data.Position != null)
        {
            _gameState.UpdatePlayerPosition(Context.ConnectionId, data.Position.X, data.Position.Y);
            await BroadcastGameState();
        }
    }

    private async Task BroadcastGameState()
    {
        var state = _gameState.GetGameState();
        await Clients.All.SendAsync("ReceiveGameState", state);
    }
}

public class GameStateService
{
    private readonly ConcurrentDictionary<string, Player> _players = new();

    public void AddPlayer(string playerId)
    {
        _players.TryAdd(playerId, new Player
        {
            PlayerId = playerId,
            Position = new Position { X = 0, Y = 0 },
            Score = 0
        });
    }

    public void RemovePlayer(string playerId)
    {
        _players.TryRemove(playerId, out _);
    }

    public void UpdatePlayerPosition(string playerId, float x, float y)
    {
        if (_players.TryGetValue(playerId, out var player))
        {
            player.Position = new Position { X = x, Y = y };
            player.Score += 10; // Increment score for movement
        }
    }

    public object GetGameState()
    {
        return new
        {
            Players = _players.Values.Select(p => new
            {
                p.PlayerId,
                p.Position,
                p.Score
            }).ToList(),
            Timestamp = DateTime.UtcNow
        };
    }

    public object GetLeaderboard()
    {
        return _players.Values
            .OrderByDescending(p => p.Score)
            .Take(10)
            .Select(p => new { p.PlayerId, p.Score })
            .ToList();
    }
}

public class Player
{
    public string PlayerId { get; set; }
    public Position Position { get; set; }
    public int Score { get; set; }
}

public class Position
{
    public float X { get; set; }
    public float Y { get; set; }
}

public class PlayerUpdate
{
    public string Action { get; set; }
    public Position Position { get; set; }
}