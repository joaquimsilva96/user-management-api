---
name: api-validation
description: Use when validating this repository end to end, including .NET build/test, vulnerability scans, HTTP checks, and Docker persistence checks.
---

# API Validation

## Purpose

Validate the application end to end.

## Core Commands

```bash
dotnet restore UserManagement.Api.slnx
dotnet build UserManagement.Api.slnx --no-restore -m:1
dotnet test UserManagement.Api.slnx --no-build -m:1
dotnet list UserManagement.Api package --vulnerable --include-transitive
dotnet list UserManagement.Tests package --vulnerable --include-transitive
git diff --check
```

## Local API Checks

Use EF Core migrations, never `EnsureCreated`.

Local migration command:

```bash
dotnet ef database update --project UserManagement.Api
```

Local run command:

```bash
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://127.0.0.1:5107 dotnet run --project UserManagement.Api --no-launch-profile
```

Check:

- `GET /health` returns `200`.
- Scalar loads at `/scalar/v1` in Development.
- CRUD endpoints return expected status codes:
  - `POST /api/users`: `201`, invalid input `400`, duplicate email `409`.
  - `GET /api/users`: `200`, invalid pagination `400`.
  - `GET /api/users/{id}`: `200` or `404`.
  - `PUT /api/users/{id}`: `200`, invalid input `400`, missing/inactive `404`, duplicate email `409`.
  - `DELETE /api/users/{id}`: `204`, missing/inactive `404`.
- Verify validation details, duplicate email handling, pagination metadata, and soft-delete behavior.

## Docker Checks

```bash
docker compose build
docker compose up -d
docker compose ps
curl -i http://localhost:8080/health
curl -i http://localhost:8080/scalar/v1
curl -i -X POST http://localhost:8080/api/users -H "Content-Type: application/json" -d '{"name":"Docker User","email":"docker@example.com"}'
curl -i http://localhost:8080/api/users
docker compose restart api
curl -i http://localhost:8080/api/users
docker compose down
```

Do not delete the Docker named volume unless explicitly requested.
