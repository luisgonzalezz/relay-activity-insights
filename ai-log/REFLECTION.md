# AI-Assisted Development Reflection

## Tools used

### GitHub Copilot

Used for:

- initial requirements analysis
- planning assistance
- bounded backend implementation
- database/EF Core setup
- aggregation implementation
- backend test generation
- Angular implementation
- frontend tests
- final engineering review

Copilot was treated as an implementation and review assistant, not as the decision-maker.

### Human engineer

Human review retained responsibility for:

- product interpretation
- definition of "normal"
- reporting-window semantics
- architecture and scope
- acceptance/rejection of AI proposals
- aggregate verification
- timezone behavior
- repository hygiene
- final ownership of the submission

## Specific moments where AI output was rejected or corrected

### 1. Baseline and reporting window

Copilot initially proposed:

- last 7 completed days
- compared with the previous 7-day period

That recommendation was rejected.

The product wording emphasizes a customer admin reviewing the dashboard on Monday morning. The chosen interpretation became:

- most recent completed local calendar week
- compared with the average of the previous four completed local calendar weeks

This was simpler to explain operationally and less sensitive to one anomalous prior week.

### 2. Frontend scope reduction

Copilot initially proposed:

- date-range selector
- location filter

Those were deliberately removed from the MVP.

The final user-controlled input is only the event type:

- Calls
- Leads
- Appointments

This protects the 4–6 hour timebox and preserves the core product value: seeing which locations need attention.

### 3. EF Core naming mismatch

The first migration generated PascalCase column names while the provided canonical seed SQL used snake_case names such as:

- account_id
- occurred_at
- created_at

The generated schema was therefore incompatible with the supplied seed data.

The EF mappings and migration metadata were corrected, the stale SQLite database was removed, and the migration/seed flow was re-run from a clean state.

### 4. Historical dataset reporting anchor

After `PLAN.md` was frozen, implementation exposed a practical issue:

the seed dataset is historical and ends in July 2026.

Using the machine's current clock would make the dashboard empty when evaluated later.

The implementation therefore uses the latest available activity timestamp for the selected account as a deterministic take-home reporting anchor.

This decision was documented after planning rather than retroactively rewriting `PLAN.md`.

In a production system, the reporting anchor would normally use the current clock plus explicit data-freshness handling.

### 5. UTC parsing in tests

A generated test fixture initially parsed UTC timestamps in a way that could shift them through the developer machine's local timezone.

The fixture was corrected to preserve explicit UTC semantics.

This was important because timezone correctness is central to the product behavior.

### 6. Sunday reporting-anchor semantics

A test expectation initially treated a Sunday anchor as though the current week were complete.

That expectation was corrected so only the last fully completed local calendar week is selected.

### 7. Final repository hygiene

The final review identified:

- missing top-level README
- tracked generated SQLite/WAL artifacts
- an empty generated backend test

These were corrected before submission.

The empty test was removed rather than kept to inflate the test count, leaving 15 meaningful backend tests.

## What AI did well

Copilot was useful for:

- surfacing ambiguities early,
- accelerating boilerplate and scaffolding,
- producing bounded implementation slices,
- suggesting useful edge cases,
- generating tests,
- performing a final repository review.

It was especially effective once repository context and task boundaries were explicit.

## Where human review mattered most

Human review was most important for:

- distinguishing product requirements from assumptions,
- choosing the reporting semantics,
- controlling scope,
- validating database compatibility,
- verifying aggregates independently,
- catching timezone-sensitive test defects,
- deciding which dependency findings justified action,
- and deciding when the solution was complete enough to stop.

## Verification approach

AI-generated code was never considered correct solely because it compiled or because generated tests passed.

Verification included:

- clean database recreation from migrations,
- seed loading,
- expected row-count validation,
- backend unit/integration tests,
- frontend tests,
- production frontend build,
- real API calls,
- direct SQLite aggregate comparisons,
- browser-level frontend/API checks,
- repository hygiene review.

Final verified results:

- 20 accounts
- 12,626 activity events
- 15 backend tests passing
- 6 Angular tests passing
- Angular production build passing

For the primary demo account, Summit Auto Group / Calls:

- current count: 34
- four-week baseline average: 28.5
- change: approximately +19.3%
- status: Typical

## Final takeaway

The main value of the agent-first workflow was speed without surrendering engineering ownership.

The AI accelerated analysis, implementation, testing, and review, while the engineer remained responsible for:

- defining the product behavior,
- constraining the implementation,
- identifying incorrect assumptions,
- verifying data correctness,
- and deciding what was safe and complete enough to submit.
