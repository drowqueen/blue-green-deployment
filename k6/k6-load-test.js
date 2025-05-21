import http from 'k6/http';
import ws from 'k6/ws';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { target: 1000000, duration: '2m' }, // Ramp to 1M VUs
        { target: 5000000, duration: '5m' }, // Peak at 5M VUs
        { target: 0, duration: '2m' }, // Ramp down
    ],
    thresholds: {
        http_req_duration: ['p(95)<100'], // 95% of requests <100ms
        http_req_failed: ['rate<0.001'], // Error rate <0.1%
        ws_connecting: ['p(95)<100'], // WebSocket connection time
    },
};

export default function () {
    // Test WebSocket (e.g., game join)
    const wsUrl = 'ws://game-server.production.svc.cluster.local';
    const wsRes = ws.connect(wsUrl, null, function (socket) {
        socket.on('open', () => {
            socket.send(JSON.stringify({ action: 'join', playerId: 'test' }));
            socket.close();
        });
    });
    check(wsRes, { 'WebSocket connected': (r) => r.status === 101 });

    // Test API (e.g., health check)
    const httpRes = http.get('http://game-server.production.svc.cluster.local/health');
    check(httpRes, { 'HTTP status 200': (r) => r.status === 200 });

    sleep(1); // Simulate user think time
}