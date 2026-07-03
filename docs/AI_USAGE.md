# AI-Assisted Development Workflow

This repository was built with AI assistance, but not by autonomous delivery. AI output was reviewed, tested, and accepted by a human before Git operations.

## Workflow Used

The development workflow was:

1. Discuss and challenge requirements and architecture with ChatGPT.
2. Define a narrow implementation phase.
3. Generate a detailed prompt for Codex.
4. Let Codex implement only that phase.
5. Review every changed file manually.
6. Run build, automated tests, vulnerability checks, and `git diff` checks.
7. Perform manual HTTP or Docker validation where relevant.
8. Commit and push only after human verification.

## Roles

ChatGPT was used for requirements analysis, architectural challenge, phase planning, prompt construction, and review support.

Codex was used for scoped implementation, test execution, validation commands, and concise implementation summaries.

The human developer retained responsibility for final architectural decisions, code review, manual testing, Git operations, and acceptance.

## Phase Boundaries

Work was intentionally split into narrow phases, including:

- project setup;
- domain model and persistence;
- application service and DTOs;
- HTTP API, validation, error handling, and integration tests;
- Docker support and self-contained startup;
- AI guidance and documentation.

Each phase had explicit constraints about what not to implement yet. This reduced accidental scope expansion and made review easier.

## Safeguards

The workflow used several safeguards against blind code generation:

- each phase began by inspecting the repository and current tooling;
- Codex was asked to preserve existing architecture and avoid unrelated refactoring;
- tests used SQLite instead of EF Core InMemory for persistence behavior;
- vulnerability scans were run after dependency changes;
- manual HTTP and Docker checks were performed for runtime behavior;
- `git diff` and `git diff --check` were inspected before completion;
- generated database files and build artifacts were kept out of the repository.

## Human-Controlled Git

Commits and pushes remained human-controlled so that source history reflected reviewed work, not raw model output. This also allowed each phase to be accepted or corrected before becoming part of the branch history.

## Repository Guidance

`AGENTS.md` and the repository-local skills under `.agents/skills/` formalize the workflow that emerged during development. They were added after the workflow was established; they did not exist from the beginning.

These files are intended to help future AI-assisted work stay consistent with the architecture, validation practices, and review expectations used in this repository.

## Limitations

AI output was not treated as authoritative. Suggestions and generated code were reviewed against the requirements, existing implementation, build results, tests, vulnerability audits, and manual validation outcomes.
