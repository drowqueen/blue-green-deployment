using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/leaderboard", () => Results.Ok(new[] { "player1", "player2" }));
app.MapHub<GameHub>("/ws");

app.Run();

public class GameHub : Hub
{
    public async Task SendMessage(string message)
    {
        var data = System.Text.Json.JsonSerializer.Deserialize<dynamic>(message);
        if (data.action == "join")
        {
            await Clients.Caller.SendAsync("ReceiveMessage", new { action = "join", status = "success" });
        }
    }
}