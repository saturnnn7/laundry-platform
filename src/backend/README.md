# Backend

The backend is a .NET 10 modular monolith. The Host is the only runnable project.
Only the skeleton exists so far: there is no module yet, only shared building blocks, health checks and API docs.

## Requirements

- .NET SDK 10.0.404 (pinned in `global.json`; a newer feature band is accepted)
- Docker, for PostgreSQL

## Layout

```
src/backend
  src/Host                      runnable web app: wiring, health checks, OpenAPI
  src/BuildingBlocks/Domain     Result and Error types
  src/BuildingBlocks/Api        endpoint registration, ProblemDetails helpers
  tests/Laundry.Tests           unit and in-memory API tests (xUnit)
```

## Database

From the repo root:

```
docker compose -f infra/compose/compose.yaml up -d postgres
```

Create `infra/compose/.env` first; see `infra/compose/README.md`.

## Configuration

The only setting for now is `ConnectionStrings:Postgres`. Set it with user-secrets (from `src/backend`):

```
dotnet user-secrets set "ConnectionStrings:Postgres" "Host=127.0.0.1;Port=5432;Database=laundry;Username=laundry;Password=<your-password>" --project src/Host
```

Or with the environment variable `ConnectionStrings__Postgres`. In PowerShell put the value in single quotes so `$` is not expanded.

Use `127.0.0.1` and not `localhost`: the compose port is published on IPv4 only, and `localhost` may resolve to IPv6 first.
Secrets never go into git.

## Build, run, test

From `src/backend`:

```
dotnet build
dotnet run --project src/Host
dotnet test
```

After start the Host listens on http://localhost:5080:
- http://localhost:5080/swagger (API docs)
- http://localhost:5080/health (liveness)
- http://localhost:5080/health/ready (readiness, checks PostgreSQL)

The app starts without a connection string; only `/health/ready` reports `Unhealthy`.
The tests need neither Docker nor secrets.

## CI

`.github/workflows/ci.yml` runs restore, build and test on every pull request to `main`.
Route list: `docs/api/rest-endpoints.md`.
