# AI Interaction 02 — PLAN.md Generation

## Tool

GitHub Copilot

## Purpose

Generate the pre-implementation `PLAN.md` after the human engineer approved the product interpretation, baseline semantics, scope, architecture constraints, and edge-case behavior.

## Prompt

```text
Thanks. I reviewed your analysis and I want to make the human product
and architecture decisions before we create PLAN.md.

The following decisions are now approved:

PRODUCT INTERPRETATION

1. The primary question is:
   "Was each location's recent activity typical compared with its own
   recent historical behavior?"

2. "Recent activity" means the last completed local calendar week
   (Monday 00:00 through the following Monday 00:00), evaluated in the
   account's configured timezone.

3. The baseline is the average weekly event count across the previous
   4 completed calendar weeks.

4. Comparisons are made per:
   - account
   - location
   - event type

5. We will NOT combine call_received, lead_created and appointment_set
   into a single normality score.

6. The user-controlled frontend input will be event type:
   - Calls
   - Leads
   - Appointments

7. Filter state must survive reload using URL query parameters, not
   localStorage.

8. All locations should remain visible even when their current-period
   activity is zero.

9. If the historical baseline is zero:
   - percentageChange = null
   - status = NoBaseline

10. Initial explainable classification:
    - BelowTypical: change < -20%
    - Typical: -20% through +20%
    - AboveTypical: change > +20%
    - NoBaseline: historical weekly average = 0

The +/-20% threshold is an MVP product heuristic, not a statistically
derived anomaly threshold. This must be documented as an assumption.

DATA / TIME

11. activity_events.occurred_at remains UTC in persistence.

12. Reporting boundaries must be calculated in the account timezone and
    converted to UTC before querying.

13. Use half-open intervals:
    [startUtc, endUtc)

14. Nullable duration_seconds and outcome values do not exclude events
    from this feature because our selected KPI is event count.

SCOPE

15. Keep the UI intentionally small:
    - account summary
    - selected event type
    - last completed week
    - 4-week baseline
    - percentage difference
    - location comparison table
    - clear status

16. Do not add a custom date-range picker for the MVP.

17. Do not add a location filter for the MVP because the core product
    value is spotting which location needs attention.

18. Do not add charts unless time remains after correctness,
    verification, tests and documentation.

ARCHITECTURE

19. Use .NET 8+ and Angular.

20. Prefer a simple feature-oriented structure.

21. Use EF Core directly from the feature/service layer.
    Do not introduce a generic repository abstraction.

22. Do not introduce:
    - MediatR
    - CQRS infrastructure
    - AutoMapper
    - NgRx
    - microservices
    - caching
    - background jobs
    - ML/forecasting

Now create PLAN.md.

Important:
- This is the pre-implementation plan.
- Do not create application code yet.
- Do not retrofit implementation details that do not exist.
- Clearly distinguish explicit challenge requirements from our working
  assumptions.
- Include trade-offs and intentionally deferred work.
- Include the aggregation edge cases that must be tested.
- Include a practical implementation sequence appropriate to the
  4–6 hour limit.

After creating PLAN.md, stop. Do not scaffold or implement anything.
```

## Copilot-generated PLAN.md

```markdown
**# PLAN.md**

**## 1. Objective**

Implement a small but correct MVP for the product question:

"Was each location's recent activity typical compared with its own recent historical behavior?"

The MVP must help a customer admin understand whether the most recent completed local calendar week is typical for each location, relative to that location's own previous 4 completed calendar weeks.

This is intentionally scoped to a single dashboard slice: account summary + location comparison + event-type selector for a fixed last-week/4-week baseline.

**## 2. Explicit challenge requirements vs working assumptions**

**### Explicit challenge requirements**

\- Backend: at least one API endpoint performing real aggregation over the seed data.

\- Frontend: Angular SPA with at least one user-controlled input and state persisted via URL query parameters.

\- Database: relational schema and migrations loaded from seed data.

\- Tests: runnable tests covering meaningful behavior.

\- AI workflow: \`PLAN.md\` written before implementation and AI interaction log captured.

\- Timebox: 4–6 hours focused work.

\- Keep scope small and correct rather than broad and fragile.

\- No ML/forecasting or alerting work.

\- No custom auth requirement; a trusted admin context is acceptable.

**### Approved working assumptions**

\- Primary comparison unit is account + location + event type.

\- "Recent activity" = the most recent completed local calendar week, Monday 00:00 through next Monday 00:00, in the account timezone.

\- Baseline = average weekly event count across the previous 4 completed calendar weeks.

\- \`call_received\`, \`lead_created\`, and \`appointment_set\` are never combined into one normality score.

\- The frontend user-controlled input is the event type selector.

\- Query-string state is used instead of localStorage.

\- All locations remain visible even if current activity is zero.

\- If historical weekly average is zero, then \`percentageChange = null\` and status is \`NoBaseline\`.

\- Classification thresholds are:

  - BelowTypical: change < -20%

  - Typical: -20% through +20%

  - AboveTypical: change > +20%

  - NoBaseline: historical weekly average = 0

\- The +/-20% rule is a product heuristic, not a statistical anomaly model; it must be documented as an assumption.

\- Missing \`duration_seconds\` and missing \`outcome\` do not exclude events from this feature because the KPI is event count.

**## 3. Product behavior to deliver**

The customer admin should be able to:

\- select an event type (Calls, Leads, Appointments),

\- see the current completed local week for the account,

\- see the 4-week historical baseline for each location,

\- understand whether each location is BelowTypical, Typical, or AboveTypical,

\- identify which locations need attention quickly,

\- and inspect the data without exporting or calling an account manager.

The MVP should emphasize explainability over polish.

**## 4. Scope boundaries**

In scope:

\- account summary,

\- selected event type,

\- last completed week,

\- 4-week historical baseline,

\- percentage difference,

\- location comparison table,

\- clear status classification.

Out of scope for the MVP:

\- custom date range picker,

\- location filter,

\- charts unless after correctness and tests are done,

\- alerting/notifications,

\- forecasting or ML,

\- generic repository abstraction,

\- CQRS/Mediator patterns,

\- heavy frontend state management,

\- advanced caching or background jobs.

**## 5. Data model and reporting logic**

**### Persistence assumptions**

\- \`activity_events.occurred_at\` remains UTC in storage.

\- Account timezone is already in \`accounts.timezone\`.

\- Reporting boundaries are computed in the account timezone and then converted to UTC before querying.

\- Use half-open intervals: \`[startUtc, endUtc)\`.

**### Aggregation target**

For each account and each location, for each event type, compute:

\- current completed local week event count,

\- historical 4-week average event count,

\- percentage change = ((current - baseline) / baseline) \* 100 where baseline > 0,

\- status classification based on the approved heuristic.

The dashboard should be able to show all locations, even when the current period count is zero.

**## 6. Backend approach**

**### Technology**

\- .NET 8+ backend.

\- EF Core directly in the feature/service layer.

\- No generic repository abstraction.

**### Feature structure**

Keep the implementation feature-oriented, not framework-heavy:

\- feature-specific controller(s)

\- feature-specific service(s)

\- EF Core queries in the service/feature layer

\- DTOs for the response model

**### Endpoint contract**

A single focused endpoint is enough for the MVP, for example:

\- GET /api/location-normality?accountId={id}&eventType={calls|leads|appointments}

The response should provide:

\- account summary,

\- selected event type,

\- current week label,

\- baseline window label,

\- location rows containing:

  - location name,

  - currentPeriodCount,

  - historicalAverage,

  - percentageChange,

  - status,

  - maybe a brief explanation string.

**### Aggregation rules**

\- Compute the current week in the account timezone.

\- Compute the prior 4 completed calendar weeks in the same timezone.

\- Build the UTC interval boundaries for each week.

\- Filter \`activity_events\` by \`account_id\` and \`event_type\`.

\- Aggregate by \`location\`.

\- Every location should be present in the output, even when count = 0.

\- If historical average is 0, set \`percentageChange = null\`, \`status = NoBaseline\`.

**## 7. Frontend approach**

**### Stack**

\- Angular frontend.

\- Minimal feature-specific component structure.

\- Query-string-driven state for selected event type.

**### UI**

Keep the UI intentionally small:

\- account summary,

\- event type selector,

\- last completed week / baseline context,

\- location comparison table,

\- clear status indicator.

**### State persistence**

Use URL query parameters for the selected event type, so the state survives a page reload.

Example shape:

\- \`?eventType=calls\`

No localStorage under the approved MVP decisions.

**## 8. Testing strategy**

Tests should cover the product-critical logic and aggregation behavior, not broad UI snapshots.

**### Backend tests**

Must cover:

\- correct current-week boundaries in account timezone,

\- correct historical baseline window selection,

\- correct event-type filtering per account/location,

\- zero-current and zero-baseline behavior,

\- \`NoBaseline\` handling,

\- correct percentage calculations,

\- status classification thresholds,

\- all locations visible even when current count is zero,

\- null values in \`duration_seconds\` and \`outcome\` do not affect event count.

**### Edge cases to test**

\- historical baseline = 0

\- current week count = 0 while baseline > 0

\- location with no activity in the current week

\- sparse or incomplete data periods

\- account with multiple locations

\- week boundaries crossing month/year

\- DST/timezone transitions if relevant to the account timezone library behavior

\- intervals computed with half-open semantics \`[startUtc, endUtc)\`

**### Frontend tests**

\- event type selector updates URL query parameter,

\- page reload preserves selected event type,

\- location table renders statuses and percentages correctly,

\- no crash when a location has \`NoBaseline\`.

**## 9. Trade-offs and deliberate deferrals**

**### Accepted trade-offs**

\- Use a product heuristic (+/-20%) rather than a statistical anomaly model.

\- Keep the UI minimal and intentionally not rich in charts.

\- Use account-local week boundaries, because the product requirement explicitly says weekly evaluation in the account timezone.

\- Prioritize correctness and explainability over visual polish.

**### Deferred work**

\- custom date range picker,

\- location filtering,

\- charting,

\- alerting and notifications,

\- forecasting or ML,

\- generic architecture abstractions,

\- advanced caching or background jobs.

These are intentionally out of scope for the 4–6 hour timebox and the product’s stated goals.

**## 10. Implementation sequence (appropriate to 4–6 hours)**

**### Phase 1: confirm aggregation contract and tests**

1\. Confirm the baseline definition and status rules.

2\. Write the backend tests for current-vs-baseline logic before implementation.

3\. Define the response DTO shape.

**### Phase 2: implement data access and aggregation**

4\. Add the feature endpoint and service.

5\. Compute current week and previous 4 weeks in account timezone.

6\. Convert those local boundaries to UTC.

7\. Filter and aggregate by account, location, and event type.

8\. Ensure zero-baseline and zero-activity cases are handled explicitly.

**### Phase 3: UI implementation**

9\. Add the Angular view for account summary and location table.

10\. Add the event type selector bound to URL query parameters.

11\. Render status labels and percentage changes clearly.

**### Phase 4: validation and docs**

12\. Validate against the seed data and edge cases.

13\. Check that all locations remain visible.

14\. Document the assumptions, scope, and product interpretation in the README.

15\. Prepare the AI interaction log and final review.

**## 11. Risk areas to treat carefully**

These are the main correctness risks that must be handled explicitly:

\- timezone conversion mistakes,

\- date boundary off-by-one errors,

\- incorrect handling of zero historical averages,

\- division-by-zero in percentage calculations,

\- failing to include all locations,

\- combining event types erroneously,

\- ignoring null-valued metrics that are not relevant to this KPI,

\- using calendar date logic inconsistent with the account timezone.

**## 12. Final implementation intent**

The implementation should produce a small, correct, explainable dashboard that answers the product question directly:

"For each location, is this week's activity typical compared with its recent historical behavior?"

The goal is not maximum feature breadth; it is a clear, valid answer that the customer admin can trust.
```

## Human review

The generated plan was accepted in overall direction, but three corrections were required before implementation.

### Accepted

- The plan correctly centers the feature on the last completed local calendar week compared with a 4-week historical baseline.
- The aggregation grain is correctly defined as account + location + event type.
- Timezone-aware local reporting boundaries converted to UTC and half-open intervals are explicitly documented.
- Zero-current and zero-baseline behavior are called out as first-class correctness cases.
- The frontend scope is intentionally small and uses an event-type selector.
- The architecture avoids unnecessary patterns and keeps EF Core close to the feature/service layer.
- Tests prioritize aggregation correctness over broad UI coverage.

### Corrections requested before implementation

#### 1. URL query parameters are not an explicit challenge requirement

The challenge only requires that at least one user-controlled input survive a page reload.

Using URL query parameters is our design decision, not an explicit requirement.

The plan should move this detail from the "Explicit challenge requirements" section into "Approved working assumptions / design decisions."

#### 2. Define what "all locations" means with the available schema

The seed schema has `accounts` and `activity_events`, but no separate `locations` dimension/table.

Therefore the implementation cannot claim knowledge of locations that have never appeared in the available event data.

For this MVP, the comparison location set should be defined as:

- the union of distinct locations observed in the current completed week or any of the four baseline weeks for the selected account and event type.

This guarantees that a location with baseline activity and zero current activity remains visible, while avoiding assumptions about locations that do not exist in the provided schema.

#### 3. Make the database/migration strategy explicit

The plan states that a relational schema and migrations are required, but it does not make the implementation choice concrete or include migration/seed setup clearly in the implementation sequence.

Approved decision:

- Use SQLite with EF Core for reviewer portability and minimal setup.
- Recreate the provided two-table relational model through EF Core migrations.
- Load the provided seed data into that schema as part of documented local setup.
- Document that SQL Server is the production-team database, but SQLite was chosen for the take-home to protect the 4–6 hour timebox.

The implementation sequence should include database scaffold, first migration, and seed loading before relying on the aggregation endpoint.

### Minor wording cleanup

Where the plan says "current completed local week", prefer "most recent completed local calendar week" for clarity.

### Approval status

The plan is approved after the three corrections above are applied, provided no application code has been written yet.


## Reflection

This interaction shows a second human review gate before implementation. The AI produced a useful and mostly correct plan, but it blurred one design decision into an explicit requirement, left an important location-source assumption implicit, and did not make the database/migration execution path concrete enough.

Those issues were corrected before coding so that the implementation would not silently invent product/domain data or under-specify a mandatory challenge deliverable.
