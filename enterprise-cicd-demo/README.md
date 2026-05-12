# ReleaseBoard

ReleaseBoard is a full-stack teaching sample for enterprise CI/CD. The app visualizes releases, environments, deployments, feature flags, audit events, and artifact identity while the repository demonstrates branch protection, pull-request validation, gated deployments, Infrastructure as Code workflows, database migrations, and security scans.

## Architecture

| Layer | Technology |
| --- | --- |
| Frontend | React, Vite, React Router |
| Backend | ASP.NET Core minimal API on .NET 9 |
| Persistence | EF Core with SQLite |
| Testing | xUnit, WebApplicationFactory, Vitest, Testing Library |
| CI/CD | GitHub Actions workflows under `.github\workflows` |
| IaC example | Azure Bicep files under `infrastructure` |

## Local setup

```powershell
dotnet restore .\ReleaseBoard.sln
dotnet run --project .\ReleaseBoard.Api
```

In another terminal:

```powershell
Set-Location .\client
npm install
npm run dev
```

Open `http://localhost:5173`. The API runs at `http://localhost:5144`.

## Useful endpoints

| Endpoint | Purpose |
| --- | --- |
| `GET /api/health/live` | Process-level liveness check |
| `GET /api/health/ready` | Readiness check that verifies database connectivity |
| `GET /api/version` | Build identity: git SHA, build number, build time, environment |
| `GET /api/releases` | Release candidates and deployment history |
| `GET /api/feature-flags` | Feature flags for progressive delivery |
| `GET /api/audit-events` | Release evidence and operational audit trail |

Mutation endpoints require the `X-ReleaseBoard-Demo-Key` header. The local default is `local-demo-key`; configure `ReleaseBoard:DemoApiKey` in production.

## Test commands

```powershell
dotnet test .\ReleaseBoard.sln
Set-Location .\client
npm test
npm run build
```

## CI/CD teaching map

| Concept | File |
| --- | --- |
| Required PR checks | `.github\workflows\pr-validate.yml` |
| Reusable build workflow | `.github\workflows\reusable-build.yml` |
| Build once, promote many | `.github\workflows\ci-build-artifact.yml` |
| Staging deployment | `.github\workflows\cd-deploy-staging.yml` |
| Gated production deployment | `.github\workflows\cd-deploy-prod.yml` |
| Database migration gate | `.github\workflows\db-migrate.yml` |
| IaC plan/apply separation | `.github\workflows\iac-plan.yml`, `.github\workflows\iac-apply.yml` |
| Nightly security checks | `.github\workflows\nightly-security.yml` |
| Ownership and branch protection | `.github\CODEOWNERS`, `.github\branch-protection.md` |
| Dependency update strategy | `.github\dependabot.yml` |

The workflows are designed to be readable teaching examples. Cloud deployment steps are parameterized with environment variables and secrets so the same patterns can be adapted to Azure App Service, Azure Container Apps, or another host.
