# PLAN.md

## 1. Objective

Implement a small but correct MVP for the product question:

"Was each location's recent activity typical compared with its own recent historical behavior?"

The MVP must help a customer admin understand whether the most recent completed local calendar week is typical for each location, relative to that location's own previous 4 completed calendar weeks.

This is intentionally scoped to a single dashboard slice: account summary + location comparison + event-type selector for a fixed last-week/4-week baseline.

## 2. Explicit challenge requirements vs working assumptions

### Explicit challenge requirements

- Backend: at least one API endpoint performing real aggregation over the seed data.
- Frontend: Angular SPA with at least one user-controlled input whose state survives a page reload.
- Database: relational schema and migrations loaded from seed data.
- Tests: runnable tests covering meaningful behavior.
- AI workflow: `PLAN.md` written before implementation and AI interaction log captured.
- Timebox: 4–6 hours focused work.
- Keep scope small and correct rather than broad and fragile.
- No ML/forecasting or alerting work.
- No custom auth requirement; a trusted admin context is acceptable.

### Approved working assumptions

- Primary comparison unit is account + location + event type.
- "Recent activity" = the most recent completed local calendar week, Monday 00:00 through next Monday 00:00, in the account timezone.
- Baseline = average weekly event count across the previous 4 completed calendar weeks.
- `call_received`, `lead_created`, and `appointment_set` are never combined into one normality score.
- The frontend user-controlled input is the event type selector.
- Query-string state is used instead of localStorage.
- The comparison location set is the union of distinct locations observed in either the most recent completed local calendar week or any of the four baseline weeks for the selected account and event type.
- This ensures that locations with historical activity but zero current activity remain visible.
- If historical weekly average is zero, then `percentageChange = null` and status is `NoBaseline`.
- Classification thresholds are:
  - BelowTypical: change < -20%
  - Typical: -20% through +20%
  - AboveTypical: change > +20%
  - NoBaseline: historical weekly average = 0
- The +/-20% rule is a product heuristic, not a statistical anomaly model; it must be documented as an assumption.
- Missing `duration_seconds` and missing `outcome` do not exclude events from this feature because the KPI is event count.

## 3. Product behavior to deliver

The customer admin should be able to:

- select an event type (Calls, Leads, Appointments),
- see the most recent completed local calendar week for the account,
- see the 4-week historical baseline for each location,
- understand whether each location is BelowTypical, Typical, or AboveTypical,
- identify which locations need attention quickly,
- and inspect the data without exporting or calling an account manager.

The MVP should emphasize explainability over polish.

## 4. Scope boundaries

In scope:

- account summary,
- selected event type,
- most recent completed local calendar week,
- 4-week historical baseline,
- percentage difference,
- location comparison table,
- clear status classification.

Out of scope for the MVP:

- custom date range picker,
- location filter,
- charts unless after correctness and tests are done,
- alerting/notifications,
- forecasting or ML,
- generic repository abstraction,
- CQRS/Mediator patterns,
- heavy frontend state management,
- advanced caching or background jobs.

## 5. Data model and reporting logic

### Persistence assumptions

- `activity_events.occurred_at` remains UTC in storage.
- Account timezone is already in `accounts.timezone`.
- Reporting boundaries are computed in the account timezone and then converted to UTC before querying.
- Use half-open intervals: `[startUtc, endUtc)`.

### Aggregation target

For each account and each location, for each event type, compute:

- most recent completed local calendar week event count,
- historical 4-week average event count,
- percentage change = ((current - baseline) / baseline) * 100 where baseline > 0,
- status classification based on the approved heuristic.

The comparison location set is the union of distinct locations observed in either the most recent completed local calendar week or any of the four baseline weeks for the selected account and event type.

This ensures all relevant locations remain visible, including locations with historical activity but zero current activity.

## 6. Backend approach

### Technology

- .NET 8+ backend.
- SQLite database for the take-home.
- EF Core directly in the feature/service layer.
- EF Core migrations for schema evolution.
- SQLite is chosen for reviewer portability and to protect the 4–6 hour timebox; the production team uses SQL Server, but SQL Server setup is intentionally not required for this take-home.
- No generic repository abstraction.

### Feature structure

Keep the implementation feature-oriented, not framework-heavy:

- feature-specific controller(s)
- feature-specific service(s)
- EF Core queries in the service/feature layer
- DTOs for the response model

### Endpoint contract

A single focused endpoint is enough for the MVP, for example:

- GET /api/location-normality?accountId={id}&eventType={calls|leads|appointments}

The response should provide:

- account summary,
- selected event type,
- current week label,
- baseline window label,
- location rows containing:
  - location name,
  - currentPeriodCount,
  - historicalAverage,
  - percentageChange,
  - status,
  - maybe a brief explanation string.

### Aggregation rules

- Compute the current week in the account timezone.
- Compute the prior 4 completed calendar weeks in the same timezone.
- Build the UTC interval boundaries for each week.
- Filter `activity_events` by `account_id` and `event_type`.
- Aggregate by `location`.
- Every location in the comparison location set must be present in the output, even when its current-period count is zero.
- If historical average is 0, set `percentageChange = null`, `status = NoBaseline`.

## 7. Frontend approach

### Stack

- Angular frontend.
- Minimal feature-specific component structure.
- Query-string-driven state for selected event type.

### UI

Keep the UI intentionally small:

- account summary,
- event type selector,
- last completed week / baseline context,
- location comparison table,
- clear status indicator.

### State persistence

Use URL query parameters for the selected event type, so the state survives a page reload.

Example shape:

- `?eventType=calls`

No localStorage under the approved MVP decisions.

## 8. Testing strategy

Tests should cover the product-critical logic and aggregation behavior, not broad UI snapshots.

### Backend tests

Must cover:

- correct current-week boundaries in account timezone,
- correct historical baseline window selection,
- correct event-type filtering per account/location,
- zero-current and zero-baseline behavior,
- `NoBaseline` handling,
- correct percentage calculations,
- status classification thresholds,
- all locations visible even when current count is zero,
- null values in `duration_seconds` and `outcome` do not affect event count.

### Edge cases to test

- historical baseline = 0
- current week count = 0 while baseline > 0
- location with no activity in the current week
- sparse or incomplete data periods
- account with multiple locations
- week boundaries crossing month/year
- DST/timezone transitions if relevant to the account timezone library behavior
- intervals computed with half-open semantics `[startUtc, endUtc)`

### Frontend tests

- event type selector updates URL query parameter,
- page reload preserves selected event type,
- location table renders statuses and percentages correctly,
- no crash when a location has `NoBaseline`.

## 9. Trade-offs and deliberate deferrals

### Accepted trade-offs

- Use a product heuristic (+/-20%) rather than a statistical anomaly model.
- Keep the UI minimal and intentionally not rich in charts.
- Use account-local calendar-week boundaries as an approved design assumption because customer reporting should align with the account's business timezone rather than arbitrary UTC boundaries.
- Prioritize correctness and explainability over visual polish.

### Deferred work

- custom date range picker,
- location filtering,
- charting,
- alerting and notifications,
- forecasting or ML,
- generic architecture abstractions,
- advanced caching or background jobs.

These are intentionally out of scope for the 4–6 hour timebox and the product’s stated goals.

## 10. Implementation sequence (appropriate to 4–6 hours)

### Phase 1: scaffold database and confirm aggregation contract

1. Scaffold the backend and database project structure.
2. Create the initial EF Core migration for the relational schema.
3. Apply the migration to SQLite.
4. Load the provided seed data into the SQLite schema.
5. Confirm the baseline definition and status rules.
6. Write the backend tests for current-vs-baseline logic before implementing the aggregation endpoint.
7. Define the response DTO shape.

### Phase 2: implement data access and aggregation

8. Add the feature endpoint and service.
9. Compute the most recent completed local calendar week and previous 4 completed calendar weeks in the account timezone.
10. Convert those local boundaries to UTC.
11. Filter and aggregate by account, location, and event type.
12. Ensure zero-baseline and zero-activity cases are handled explicitly.

### Phase 3: UI implementation

13. Add the Angular view for account summary and location table.
14. Add the event type selector bound to URL query parameters.
15. Render status labels and percentage changes clearly.

### Phase 4: validation and docs

16. Validate against the seed data and edge cases.
17. Check that all locations remain visible.
18. Document the assumptions, scope, and product interpretation in the README.
19. Prepare the AI interaction log and final review.

## 11. Risk areas to treat carefully

These are the main correctness risks that must be handled explicitly:

- timezone conversion mistakes,
- date boundary off-by-one errors,
- incorrect handling of zero historical averages,
- division-by-zero in percentage calculations,
- failing to include all locations,
- combining event types erroneously,
- ignoring null-valued metrics that are not relevant to this KPI,
- using calendar date logic inconsistent with the account timezone.

## 12. Final implementation intent

The implementation should produce a small, correct, explainable dashboard that answers the product question directly:

"For each location, is this week's activity typical compared with its recent historical behavior?"

The goal is not maximum feature breadth; it is a clear, valid answer that the customer admin can trust.
