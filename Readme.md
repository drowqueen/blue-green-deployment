# Mock Mobile Game App Demo

## Overview
Deploys a C# game server with blue-green deployment on EKS cluster(`eks-game-demo`).

## Runbook
- **Trigger Pipeline**: Push to `main` or run in GitHub Actions.
- **Check Status**: View logs for "K6 tests failed" or "Switched traffic to green".
- **Monitor Metrics**: Access Grafana (`grafana.production.svc.cluster.local`) for latency/errors.
- **Debug Blue-Green**: Run `helm list -n production`, `kubectl get ingress -n production`.