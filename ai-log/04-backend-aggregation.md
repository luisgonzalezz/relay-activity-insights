# AI Interaction 04 — Backend Aggregation

## Tool

GitHub Copilot

## Purpose

Implement and verify the bounded backend activity-normality feature after the database foundation was approved.

The implementation scope was limited to:

- reporting-period calculation
- timezone-aware boundaries
- four-week baseline aggregation
- account and location summary calculations
- event-type validation
- backend endpoint
- meaningful backend tests
- verification against the real seeded SQLite database

No Angular work was allowed in this step.

## Copilot implementation result

```text
Implemented the backend activity-normality endpoint and its tests. No Angular, PLAN.md, or AI logs were changed.

Files changed:
- src/Relay.Api/Program.cs
- new feature files:
  - LocationNormalityEndpoint.cs
  - LocationNormalityDtos.cs
  - LocationNormalityService.cs
  - ReportingPeriodCalculator.cs
- new tests:
  - LocationNormalityEndpointTests.cs
  - LocationNormalityServiceTests.cs
  - ReportingPeriodCalculatorTests.cs

Verification:
- dotnet test tests/Relay.Api.Tests/Relay.Api.Tests.csproj --nologo
- Result: 16 passed, 0 failed

Seeded SQLite verification:
- GET /api/location-normality?accountId=1&eventType=calls
- Account: Summit Auto Group
- Timezone: America/Chicago
- Reporting week: July 20–27, 2026
- Baseline period: June 22–July 20, 2026
- Account current total: 34
- Account average weekly baseline: 28.5
- Percentage change: +19.298%
- Status: Typical

Example locations:
- Site A: 5 current / 5 baseline average
- Site B: 8 current / 4.75 baseline average
- Site E: 6 current / 6.75 baseline average

Separate direct SQLite queries grouped by location and by each of the four baseline weeks independently matched the endpoint results and the 34-event account total.

Tests cover:
- local-week boundaries
- timezone and DST boundaries
- half-open intervals
- four-week denominator behavior
- zero current activity
- zero baselines
- threshold boundaries
- per-location isolation
- event-type filtering
- nullable duration/outcome
- unsupported event types
- unknown accounts
- accounts without activity

Corrections made during implementation:
- Fixed a test fixture UTC parsing issue that was shifting timestamps through local time.
- Corrected the Sunday-anchor expectation to use the last fully completed week.
```

## Human review

### Accepted

- The feature stayed within the approved backend-only scope.
- The reporting anchor uses the account's latest available activity rather than the machine clock, making the historical seed deterministic for review.
- The latest available timestamp is based on account activity rather than the selected event type, so changing the event selector does not shift the reporting period.
- The reporting week and four-week baseline are timezone-aware.
- Baseline averages use exactly four calendar weeks rather than averaging only non-empty weeks.
- Location rows are isolated correctly and locations with historical activity but zero current activity remain representable.
- Unsupported event types and unknown accounts are explicitly handled.
- Nullable `duration_seconds` and `outcome` do not alter count-based aggregation.
- Tests cover product-critical rules rather than superficial code coverage.
- The endpoint was not accepted solely because tests passed; results were independently checked with direct SQLite aggregation.

### Verification against seeded data

For account 1 (`Summit Auto Group`, `America/Chicago`) and `calls`:

- most recent completed local calendar week: July 20–27, 2026
- four-week baseline period: June 22–July 20, 2026
- current account total: 34
- baseline weekly average: 28.5
- percentage change: approximately +19.298%
- status: `Typical`

The direct database verification matched the API output.

This was an important acceptance gate because aggregate correctness is one of the main evaluation criteria in the challenge.

### AI / implementation issues caught and corrected

#### 1. UTC parsing in a test fixture

A test fixture initially parsed UTC timestamps in a way that allowed conversion through the machine's local timezone, shifting the timestamps.

That behavior was corrected so test timestamps preserve explicit UTC semantics.

Why this matters:

Timezone behavior is central to this feature. A test suite that accidentally depends on the developer machine's local timezone could pass locally while validating the wrong reporting boundaries.

#### 2. Sunday reporting-anchor expectation

An initial test expectation treated the Sunday anchor incorrectly.

The expectation was corrected so the calculator selects the last fully completed local calendar week rather than treating an in-progress week as complete.

Why this matters:

The product definition explicitly requires a completed reporting week. Partial-week comparisons would produce misleading normality signals.

### Additional human check

One boundary worth preserving in the test suite is an anchor exactly at local Monday 00:00. At that instant, the week that just ended is fully complete and should become the reporting week. This protects the boundary between "Sunday still in progress" and "Monday new week started."

## Decision

Backend aggregation is approved to move forward to the Angular slice.

The implementation now has:

- deterministic historical reporting
- timezone-aware weekly boundaries
- verified four-week baselines
- explicit zero/no-baseline handling
- meaningful backend tests
- real seed verification

No additional backend features should be added before the frontend unless a defect is discovered.

## Reflection

This step produced two useful examples of why AI-generated implementation still requires engineering review.

Neither issue was architectural, but both were correctness-sensitive:

- timestamp parsing could have invalidated timezone tests,
- reporting-anchor semantics could have allowed incomplete periods.

The implementation was accepted only after both were corrected and the final aggregates were independently verified against the database.
