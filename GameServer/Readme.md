## Program.cs Information

This game server is a minimal mockup but supports real-time multiplayer interactions via SignalR, making it suitable for K6 load testing.

### SignalR Hub (GameHub)
- Handles player connections/disconnections using OnConnectedAsync and OnDisconnectedAsync.
- Processes player updates (e.g., movement) via SendPlayerUpdate, updating position and score.
- Broadcasts the game state to all connected clients after each update.

### GameStateService
- Manages player data in a ConcurrentDictionary for thread-safe operations.
- Tracks player positions and scores, simulating game mechanics.
- Provides methods to get the current game state and leaderboard.

### Endpoints
- /health: Returns server status and timestamp.
- /leaderboard: Returns top 10 players by score.
- /game-state: Returns current game state (all players' positions and scores).
- /ws: SignalR WebSocket endpoint for real-time communication.

### CORS
Added to allow cross-origin requests for testing purposes.