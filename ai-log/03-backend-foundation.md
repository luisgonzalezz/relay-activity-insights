# AI Interaction 03 — Backend / Database Foundation

## Tool

GitHub Copilot

## Purpose

Implement only the backend/database foundation from the approved `PLAN.md`:

- .NET 8+ ASP.NET Core API
- SQLite
- EF Core
- EF Core migrations
- deterministic seed loading
- minimal test project
- no aggregation endpoint
- no frontend

## Copilot implementation result

```text
Files changed

- src/Relay.Api/Relay.Api.csproj
- src/Relay.Api/Program.cs
- src/Relay.Api/appsettings.json
- src/Relay.Api/Data/RelayDbContext.cs
- src/Relay.Api/Data/DatabaseSeeder.cs
- src/Relay.Api/Data/RelayDbContextFactory.cs
- src/Relay.Api/Data/Migrations/20261001143810_InitialCreate.cs
- src/Relay.Api/Data/Migrations/20261001143810_InitialCreate.Designer.cs
- src/Relay.Api/Data/Migrations/RelayDbContextModelSnapshot.cs
- tests/Relay.Api.Tests/Relay.Api.Tests.csproj
- tests/Relay.Api.Tests/DatabaseFoundationTests.cs
- solution file: Relay.ActivityInsights.slnx

Commands executed

- dotnet test tests/Relay.Api.Tests/Relay.Api.Tests.csproj --nologo
- dotnet ef database update --project src/Relay.Api/Relay.Api.csproj --startup-project src/Relay.Api/Relay.Api.csproj --no-build
- dotnet run --project src/Relay.Api/Relay.Api.csproj --urls http://localhost:5012
- Invoke-WebRequest -Uri http://localhost:5012/health -UseBasicParsing

Verification results

- Migration + seed path works with SQLite.
- API started successfully and health endpoint returned:
  {"status":"ok"}
- Database foundation test passed:
  - 20 accounts
  - 12,626 activity_events
- Test summary: 2 passed, 0 failed

Assumptions / deviations

- No deviation from the approved plan.
- The only issue was a schema mismatch introduced by the initial migration: it generated PascalCase columns while the provided SQL seed inserts snake_case columns like created_at, account_id, occurred_at, etc.
- I corrected the EF model and migration metadata to match the canonical schema and reset the stale SQLite database before re-running startup verification.

This is the backend/database foundation only; no aggregation endpoint, frontend, charts, or extra infrastructure were added.
```

## Human review

### Accepted

- Scope remained bounded to the backend/database foundation.
- The provided relational model was reproduced with EF Core migrations.
- SQLite was used as approved for reviewer portability.
- The seed path was verified against the expected dataset totals:
  - 20 accounts
  - 12,626 activity events
- The API started successfully and exposed a minimal health check.
- Foundation tests passed.
- No aggregation endpoint, Angular code, charts, or unnecessary infrastructure were introduced.

### AI issue caught and corrected

The initial EF Core migration generated PascalCase column names while the canonical seed SQL referenced snake_case names such as:

- `created_at`
- `account_id`
- `occurred_at`

This made the generated database schema incompatible with the provided seed data.

The mismatch was not accepted as an implementation detail. The EF mapping and migration metadata were corrected to match the canonical provided schema, the stale SQLite database was reset, and the migration + seed flow was re-run from a clean state.

### Why this mattered

The challenge explicitly requires the provided seed data to be loaded into a real relational schema with migrations. A schema that only works with a rewritten seed or an already-existing database would weaken reproducibility and could hide mapping defects.

The corrected implementation now treats the starter schema as the source of truth rather than allowing EF naming defaults to silently change it.

### Follow-up verification before moving on

Before implementing aggregation, the following should remain true:

- a clean database can be created entirely from migrations,
- seed loading succeeds without manual edits during startup,
- rerunning the documented setup is deterministic,
- the expected row counts remain 20 accounts / 12,626 events,
- `PLAN.md` remains unchanged.

## Reflection

This was a useful agent-review moment because the generated foundation was not correct on the first attempt. The problem was concrete and testable: EF naming conventions produced a schema incompatible with the provided SQL seed.

The implementation was only accepted after the schema was aligned with the canonical input and the setup was re-verified from a clean SQLite database.

This reinforces the intended workflow for the exercise:

AI generates implementation → engineer verifies against source data → defect is identified → AI is redirected/corrected → clean verification is repeated before proceeding.
