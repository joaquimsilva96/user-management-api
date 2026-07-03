# Repository Guidance

## Architecture

- This is a flat, pragmatic ASP.NET Core Web API solution targeting .NET 10.
- Keep application code in `UserManagement.Api` and tests in `UserManagement.Tests`.
- Controllers delegate to `IUserService`; controllers should not contain business logic.
- `UserService` uses `ApplicationDbContext` directly. Do not introduce a repository pattern, MediatR, CQRS, or AutoMapper without explicit justification.
- Use async APIs and pass `CancellationToken` through EF Core and service calls.

## Domain And Persistence

- Preserve soft-delete behavior: inactive users are excluded from reads and delete sets `IsActive = false`.
- Email uniqueness must remain enforced both proactively in service logic and finally by the database unique index.
- Use EF Core migrations for schema changes. Never use `EnsureCreated` in application startup.
- SQLite is the persistence provider for this assessment.

## HTTP And Validation

- Preserve RFC-compatible `ProblemDetails` and `ValidationProblemDetails` responses.
- Preserve explicit FluentValidation usage; do not add DataAnnotations for the same request rules.
- Keep OpenAPI and Scalar available in Development.

## Tests

- Tests use SQLite, not the EF Core InMemory provider.
- Add focused tests for changed behavior. Do not weaken existing persistence, service, or integration tests.
- Keep integration tests isolated and independent of execution order.

## Required Validation

Run relevant validation before completing work:

```bash
dotnet restore UserManagement.Api.slnx
dotnet build UserManagement.Api.slnx --no-restore -m:1
dotnet test UserManagement.Api.slnx --no-build -m:1
dotnet list UserManagement.Api package --vulnerable --include-transitive
dotnet list UserManagement.Tests package --vulnerable --include-transitive
git diff --check
```

For Docker changes, also run:

```bash
docker compose build
docker compose up -d
docker compose ps
curl -i http://localhost:8080/health
curl -i http://localhost:8080/scalar/v1
curl -i http://localhost:8080/api/users
docker compose down
```

## Workflow Rules

- Inspect existing patterns before editing.
- Keep changes limited to the requested phase.
- Inspect `git diff` and run `git diff --check` before completion.
- Never commit, push, change branches, or rewrite history unless explicitly instructed.
- Stop after the requested phase and report changed files plus validation results.
