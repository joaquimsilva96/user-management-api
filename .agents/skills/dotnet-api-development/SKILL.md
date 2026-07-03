---
name: dotnet-api-development
description: Use when implementing or modifying a narrow ASP.NET Core API feature in this repository while preserving the current architecture and validation workflow.
---

# Dotnet API Development

## Purpose

Implement or modify one narrow API feature while respecting the current architecture.

## Workflow

1. Inspect existing patterns before editing.
2. Identify affected layers: controller, model, validator, service, persistence, tests, or Docker/config.
3. Preserve public API behavior unless the request explicitly changes it.
4. Keep controllers thin and delegate business behavior to `IUserService`.
5. Keep `UserService` direct-to-`ApplicationDbContext`; do not add repository pattern, MediatR, CQRS, or AutoMapper.
6. Use async EF Core APIs and pass `CancellationToken`.
7. Add or update focused tests for the changed behavior.
8. Avoid unrelated refactoring, formatting churn, or migration edits.

## Validation

Run the smallest useful validation first, then the full requested validation:

```bash
dotnet build UserManagement.Api.slnx --no-restore -m:1
dotnet test UserManagement.Api.slnx --no-build -m:1
git diff --check
```

Report assumptions, trade-offs, changed files, and validation results.
