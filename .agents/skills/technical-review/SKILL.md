---
name: technical-review
description: Use when reviewing a completed implementation phase in this repository before a human-controlled commit.
---

# Technical Review

## Purpose

Review a completed phase before commit.

## Review Checklist

- Inspect every changed file.
- Check architecture consistency:
  - controllers depend on `IUserService`;
  - `UserService` uses `ApplicationDbContext` directly;
  - no repository pattern, MediatR, CQRS, or AutoMapper was introduced without justification.
- Check validation and exception behavior:
  - explicit FluentValidation remains in use;
  - `ProblemDetails` responses stay consistent;
  - duplicate email remains a `409`.
- Check for secrets, generated files, local database files, and build artifacts.
- Check test quality, isolation, and SQLite usage.
- Check migration safety:
  - EF Core migrations only;
  - no `EnsureCreated` in application startup.
- Check Docker behavior:
  - final image runs non-root;
  - SQLite data persists through the named volume;
  - healthcheck works without adding large runtime tools.
- Check build warnings and vulnerable dependencies.
- Identify overengineering or unrelated refactoring.

## Output

Provide blocking findings first, then non-blocking findings. Include file and line references when possible.

Do not modify files unless explicitly requested.
