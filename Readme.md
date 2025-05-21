# Mock Mobile Game App Demo

## Overview
Deploys a C# game server with blue-green deployment on EKS cluster(`eks-game-demo`).

## Assumptions
- SonarQube is running in the EKS cluster, accessible at http://sonarqube.production.svc.cluster.local:9000 and the SonarQube instance is configured with a project named mock-game-app
-  A SonarQube token is stored in GitHub Secrets as SONAR_TOKEN, which is safe for authentication purposes.
- The GameServer.Tests project is configured to generate code coverage reports in OpenCover format (e.g., using coverlet).


## Runbook
- **Trigger Pipeline**: Push to `main` or run in GitHub Actions.
- **Check Status**: View logs for "K6 tests failed" or "Switched traffic to green".
- **Monitor Github actions** Check the logs of github actions.
- **Monitor SonarQube** Check for code quality results.
- **Monitor Metrics**: Access Grafana (`grafana.production.svc.cluster.local`) for latency/errors.
- **Debug Blue-Green**: Run `helm list -n production`, `kubectl get ingress -n production`.

See the Readme files under GameServer/ and k6/ for further information.