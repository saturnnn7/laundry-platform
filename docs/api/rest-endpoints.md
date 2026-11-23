# REST endpoints

Feature routes live under the base path `/api/v1`. None exist yet. This file grows with each Part.

## Routes that exist now

All four are anonymous and are outside `/api/v1`.

| Method | Path | Purpose | Success | Errors | Auth |
|---|---|---|---|---|---|
| GET | `/health` | Liveness: the process answers | 200, text `Healthy` | none | anonymous |
| GET | `/health/ready` | Readiness: checks PostgreSQL | 200, text `Healthy` | 503, text `Unhealthy` | anonymous |
| GET | `/openapi/v1.json` | OpenAPI document | 200, JSON | none | anonymous |
| GET | `/swagger` | Swagger UI; redirects (301) to `/swagger/index.html` | 200, HTML at `/swagger/index.html` | none | anonymous |

The health bodies are plain text only. They never show hosts, database names or exception text.

## Errors

Errors are ProblemDetails (`application/problem+json`). The `type` field is a short slug.

Slugs set by the framework, by status:

| Status | `type` |
|---|---|
| 400 | `bad-request` |
| 401 | `unauthorized` |
| 403 | `forbidden` |
| 404 | `not-found` |
| 405 | `method-not-allowed` |
| 409 | `conflict` |
| 500 | `internal-error` |
| other | `http-<code>` |

An unknown route gives 404 `not-found`. An unhandled exception gives 500 `internal-error` with no message or stack trace.

Errors made by the application use the error code as `type` (for example `email-not-confirmed`) and the error description as `detail`.
The status comes from the error kind: Validation 400, Unauthorized 401, Forbidden 403, NotFound 404, Conflict 409, Failure 500.

## Conventions

These rules apply from the first feature route that needs them.

- Routes are versioned: `/api/v1`.
- Timestamps are UTC, ISO-8601.
- Lists use cursor pagination: `{ items, nextCursor }`.
- POST requests that create bookings, holds and session starts take an `Idempotency-Key` header.
- Errors are ProblemDetails with a `type` slug, as above.
