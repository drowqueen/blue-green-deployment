# Mock Mobile Game App Demo

## Overview
Deploys a C# game server with blue-green deployment on EKS cluster(`eks-game-demo`).

# CI/CD Pipeline Overview

The GitHub Actions workflow (`.github/workflows/blue-green-deploy.yml`) automates the deployment of the mock mobile game app to the `eks-game-demo` EKS cluster using a blue-green strategy. It ensures zero-downtime updates for mobile users, validates performance with K6 load tests, and maintains code quality with SonarQube.


## Requirements
- **Secrets**: `SONAR_TOKEN`, `REGISTRY_USERNAME`, `REGISTRY_PASSWORD`, `KUBE_CONFIG`, `SLACK_BOT_TOKEN` in GitHub Secrets.
- **Cluster**: `eks-game-demo` with K6 Operator, NGINX Ingress Controller, Prometheus, Grafana.
- **Files**: Ensure `coverlet.runsettings` is in `GameServer.Tests/` for SonarQube coverage.
- **ECR**: Replace <account-id> with your AWS account ID and us-west-2 with your EKS region in the workflow.


## Assumptions
- SonarQube is running in the EKS cluster, accessible at http://sonarqube.production.svc.cluster.local:9000 and the SonarQube instance is configured with a project named mock-game-app
- A SonarQube token is stored in GitHub Secrets as SONAR_TOKEN, which is safe for authentication purposes.
- The GameServer.Tests project is configured to generate code coverage reports in OpenCover format (e.g., using coverlet).
- The GitHub Actions runner is deployed as a pod in eks-game-demo (e.g., via the actions-runner-controller Helm chart).
- The runner’s pod uses a Kubernetes service account with an IAM role (via IRSA) that provides AWS SSO-based permissions for ECR and EKS.


## What It Does

1. **Triggers**: Runs on pushes to the `main` branch.
2. **Build & Test**:
   - Builds the C# app (.NET 8) and runs unit tests (`GameServer.sln`, `GameServer.Tests.csproj`).
   - Generates code coverage for SonarQube using `coverlet.runsettings`.
3. **SonarQube Analysis**:
   - Scans code for bugs and vulnerabilities, uploading coverage from `GameServer.Tests/coverage.opencover.xml`.
4. **Docker**:
   - Builds and pushes the app image to `registry.example.com/mock-game-app:<commit-sha>`.
5. **Deploy Green**:
   - Deploys the green environment via Helm (`charts/game-server`, `env=green`).
6. **K6 Load Testing**:
   - Runs mobile-specific tests (`k6/k6-load-test.js`) on dedicated test nodes, targeting WebSocket (`/ws`) and API endpoints (`/health`, `/leaderboard`).
   - Checks latency (<100ms) and errors (<0.1%) for 100K users (scalable to 1M–5M).
7. **Traffic Switch**:
   - If tests pass, patches the Ingress (`game-server-ingress`) to route traffic to `game-server-green`.
   - Existing WebSocket connections stay on blue until clients reconnect or cleanup.
8. **Cleanup**:
   - Waits 5 minutes, then removes the blue environment (`helm uninstall game-server-blue`).
9. **Rollback**:
   - If K6 tests fail, rolls back green (`helm rollback game-server-green`).
10. **Notification**:
    - Sends deployment status to Slack (`devops-notifications` channel).

## Key Features

- **Zero-Downtime**: Ingress-level switching ensures new connections go to green without interrupting mobile users.
- **Automated Testing**: K6 validates performance on test nodes, with metrics in Grafana.
- **Code Quality**: SonarQube enforces reliability for the C# app.
- **Team-Friendly**: Automation and Slack alerts minimize Kubernetes expertise needed.


## How to Use / Runbook

- **Trigger**: Push to `main` or run manually in GitHub Actions.
- **Monitor**:
  - Check github action logs.
  - Check SonarQube for code quality results.
  - View Slack channel `devops-notifications` for status.
  - Check  Grafana (`grafana.production.svc.cluster.local`) for metrics such as latency and errors.
- **Debug**:
  - List Helm releases: `helm list -n production`.
  - Check Ingress: `kubectl get ingress game-server-ingress -n production`.
- **Scale**: Edit `k6/k6-testrun.yaml` (e.g., `parallelism: 400`) for 1M–5M users.

See the Readme files under GameServer/ and k6/ for further information.