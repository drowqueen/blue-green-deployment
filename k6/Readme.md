## K6 Load Test Information


Adjust the K6 options (e.g., vus, duration) based on your testing needs.
Ensure your charts/game-server Helm chart exposes the service correctly (e.g., via a ClusterIP or ingress).

### HTTP Tests:
- Checks the /health endpoint for a 200 status and "healthy" response.
- Tests the /leaderboard endpoint, ensuring it returns a 200 status and an array.

### WebSocket Tests:
- Connects to the /ws endpoint.
- Sends a join message and a move message with random coordinates.
- Verifies the server responds with a successful join message and game state updates.

### Options:
- Simulates 50 virtual users for 30 seconds.
- Sets thresholds for HTTP error rates (<1%), HTTP request duration (<500ms for 95%), and WebSocket connection time (<100ms for 95%).

### Assumptions:
The service is accessible at game-server.production.svc.cluster.local