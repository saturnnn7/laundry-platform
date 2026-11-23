# Local infrastructure

This folder holds the Docker Compose file for local development.
For now it runs one service: PostgreSQL 17. More services will be added here later.

Setup:
1. Copy `.env.example` to `.env` in this folder and set a password. `.env` is git-ignored.
2. Start (from the repo root): `docker compose -f infra/compose/compose.yaml up -d`.
3. Check: `docker compose -f infra/compose/compose.yaml ps` shows `healthy` when it is ready.
4. Stop: `docker compose -f infra/compose/compose.yaml down`.

PostgreSQL listens on 127.0.0.1 only, port 5432 by default (set `POSTGRES_PORT` to change it).
Data lives in the named volume `postgres-data`.
`down` keeps the data. `down -v` deletes the volume and all data.
