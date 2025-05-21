import http from 'k6/http';
import ws from 'k6/ws';
import { check, sleep } from 'k6';

export const options = {
    vus: 50, // 50 virtual users
    duration: '30s', // Run for 30 seconds
    thresholds: {
        http_req_failed: ['rate<0.01'], // HTTP error rate < 1%
        http_req_duration: ['p(95)<500'], // 95% of HTTP requests under 500ms
        ws_connecting: ['p(95)<100'], // 95% of WebSocket connections under 100ms
    },
};

export default function () {
    // Test health endpoint
    const healthRes = http.get('http://game-server.production.svc.cluster.local/health');
    check(healthRes, {
        'health check status is 200': (r) => r.status === 200,
        'health check returns healthy': (r) => r.json().status === 'healthy',
    });

    // Test leaderboard endpoint
    const leaderboardRes = http.get('http://game-server.production.svc.cluster.local/leaderboard');
    check(leaderboardRes, {
        'leaderboard status is 200': (r) => r.status === 200,
        'leaderboard returns array': (r) => Array.isArray(r.json()),
    });

    // Test WebSocket connection
    const wsRes = ws.connect('ws://game-server.production.svc.cluster.local/ws', null, function (socket) {
        socket.on('open', () => {
            // Send join message
            socket.send(JSON.stringify({ action: 'join' }));

            // Simulate player movement
            socket.send(JSON.stringify({
                action: 'move',
                position: { x: Math.random() * 100, y: Math.random() * 100 },
            }));

            // Wait for a response
            socket.setTimeout(() => {
                socket.close();
            }, 1000);
        });

        socket.on('message', (data) => {
            const msg = JSON.parse(data);
            check(msg, {
                'received join success': (m) => m.action === 'join' && m.status === 'success',
                'received game state': (m) => m.players !== undefined,
            });
        });
    });

    check(wsRes, {
        'WebSocket connection established': (r) => r && r.status === 101,
    });

    sleep(1); // Pause between iterations
}