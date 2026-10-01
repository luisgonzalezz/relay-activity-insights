# AI Interaction 01 — Requirements Analysis

## Tool

GitHub Copilot

## Purpose

Analyze the take-home challenge, schema, seed data, and repository instructions before writing `PLAN.md` or application code.

## Prompt

```text
Read the repository context before doing anything else.

Start by reviewing these files:
- docs/CHALLENGE.md
- data/schema.sql
- data/seed.sql
- .github/copilot-instructions.md

Do not write application code.
Do not scaffold the backend or frontend.
Do not create PLAN.md yet.

Act as a senior product-minded full-stack engineer and software architect.

Analyze the challenge and return:

1. Product problem
   - What problem is the ticket actually trying to solve?
   - What should the customer admin be able to understand or do after using this feature?

2. Explicit requirements
   - Backend requirements
   - Frontend requirements
   - Database requirements
   - Testing requirements
   - AI/agentic workflow requirements
   - Submission requirements

3. Ambiguities and underspecified areas
   Identify anything the ticket does not define clearly, especially:
   - what "normal" means
   - what "recent activity" means
   - comparison window
   - location-level behavior
   - account-level behavior
   - timezone/date boundaries
   - event types
   - empty periods
   - zero historical activity
   - incomplete data

4. Product questions
   List the questions you would ideally ask Product before implementation.
   Prioritize them by impact on correctness.

5. Working assumptions
   For questions we may choose not to wait on, propose reasonable assumptions.
   Clearly separate assumptions from explicit requirements.

6. Seed data analysis
   Inspect the actual schema and seed data and identify:
   - number and shape of accounts
   - multi-location examples
   - event types
   - date range
   - timezone considerations
   - nullable or unusual values
   - data quality issues
   - edge cases that could affect aggregation correctness

7. Smallest valuable vertical slice
   Propose the smallest implementation that would satisfy the ticket well within a 4–6 hour timebox.

   Do not optimize for feature count.
   Optimize for:
   - correctness
   - product usefulness
   - multi-location visibility
   - explainability
   - testability

8. Aggregation risks
   Identify technical mistakes that could produce misleading dashboard results, including:
   - timezone handling
   - date boundaries
   - missing locations
   - zero activity
   - division by zero
   - incomplete periods
   - aggregation across locations or event types

9. Architecture considerations
   Recommend only the level of architecture appropriate for a 4–6 hour take-home.
   Call out anything that would be unnecessary overengineering.

10. Suggested implementation order
    Provide a high-level implementation sequence, but do not generate code.

Important:
- Do not silently invent requirements.
- Clearly distinguish facts, assumptions, and recommendations.
- Prefer simple and explainable decisions over sophisticated statistical approaches.
- ML and forecasting are explicitly out of scope.
- Treat aggregate correctness as more important than visual polish.
- Do not modify any files yet.

At the end, include a section named:

"Decisions that require human approval"

List the product and architecture decisions that should be made by the engineer before PLAN.md is written.
```

## Copilot response

```text
I reviewed the repo context in the required files before proceeding:

- docs/CHALLENGE.md
- data/schema.sql
- data/seed.sql
- .github/copilot-instructions.md

No application code or repo files were modified in this analysis step.

1. Product problem

What problem is the ticket actually trying to solve?

- The core product question is not “how many events happened?” but “is this volume/behavior normal for this customer, especially across multiple locations?”
- The customer admin needs a fast, understandable answer to:
- “Are we unusually low or high versus our own recent pattern?”
- “Which location needs attention?”
- “Are we trending unusually compared with our recent baseline?”
- The ticket is really about helping a customer understand operational health without exporting data or calling an account manager.
- Product is explicitly targeting Monday-morning decision-making: a customer admin should see a quick health signal and identify likely action.

What should the customer admin be able to understand or do after using this feature?

- Understand whether recent activity is unusually high, low, or in line with the customer’s own baseline.
- Understand which location(s) are driving the trend or deviating from expected behavior.
- Decide where to intervene operationally (e.g., staffing, follow-up, location review).
- Reduce uncertainty in a dashboard that otherwise shows only raw totals.

1. Explicit requirements

Backend requirements

- Must expose at least one API endpoint that performs real aggregation over the seed data.
- Aggregation must go beyond pass-through queries.
- Must support a dashboard-oriented summary, not just raw rows.
- Must support customer/account-level and likely location-level breakdowns.
- Endpoint(s) should be explainable and deterministic.

Frontend requirements

- Must be an Angular SPA.
- Must include at least one user-controlled input that affects the result.
- That input state must survive a page reload.
- Use one of:
- date range,
- location selector,
- account filter,
- toggle,
- comparison period selector.
- The UI must consume the backend data and render a clear summary view.

Database requirements

- Must use a real relational schema and migrations.
- Must load from the provided seed data.
- Data model is intentionally minimal: accounts + activity_events.
- All timestamps are stored in UTC, even though display logic may convert to local time.

Testing requirements

- Must include tests that run.
- Tests should cover the meaningful aggregation logic and edge cases.
- At least a one-line note in README should explain how to run tests.

AI/agentic workflow requirements

- A PLAN.md must exist and be written before implementation.
- An AI interaction log must exist and reflect how the engineer directed, reviewed, and corrected agent output.
- The challenge explicitly values agentic discipline and product judgment over raw code volume.

Submission requirements

- Public GitHub repo.
- README with:
- local run instructions,
- interpretation of the ticket,
- key assumptions and design decisions,
- trade-offs,
- deliberately deferred items,
- what would be done with one more day.
- PLAN.md
- AI log
- Working app with backend, frontend, migrations, and tests.

1. Ambiguities and underspecified areas

This ticket is deliberately underspecified. The unresolved product definitions are the hard part.

- What “normal” means
- Historical average?
- Same account baseline?
- Same location baseline?
- Same event type baseline?
- Relative to prior period or a longer moving window?
- What “recent activity” means
- Last 7 days?
- Last 14 days?
- Rolling 30 days?
- Since Monday morning vs last completed week?
- Different default depending on customer/admin?
- Comparison window
- Current vs previous period?
- Current vs trailing average?
- Same weekday pattern?
- Last 4 weeks vs prior 4 weeks?
- Location-level behavior
- Compare each location to its own history?
- Compare each location to account average?
- Show only outliers?
- Show all locations by default?
- Account-level behavior
- Is the dashboard account-wide first, then drill into locations?
- Should the customer admin be able to change account context if multiple accounts exist?
- Timezone/date boundaries
- All timestamps are UTC, but customers operate in local time.
- “This week” for a customer in Chicago is not the same as UTC week boundaries.
- Need to decide whether date bucketing uses account timezone or UTC.
- Event types
- call_received, lead_created, appointment_set are not interchangeable.
- Should they be aggregated together? Probably yes for total activity, but likely also shown separately for operational usefulness.
- Empty periods
- If there are no events in a 7-day period, should we show zero or “no data”?
- Should we treat zero as “unusually low” or simply “inactive”?
- Zero historical activity
- A new location or new customer may have no baseline.
- Should the dashboard show “insufficient data” rather than a misleading normality signal?
- Incomplete data
- Missing duration_seconds for many call_received rows.
- Missing outcome for some lead/appointment rows.
- If the dashboard uses duration or conversion-like metrics, the system must avoid presenting an incomplete denominator as valid.

1. Product questions

Prioritized, highest-impact first:

1. What is the intended definition of “normal”?

- Same account baseline, same location baseline, prior period, trailing average, or something else?

2. What is the default view for “recent activity”?

- 7 days, 14 days, 30 days, or last completed week?

3. Should the dashboard be primarily:

- account-level summary,
- location-level drill-down,
- or both?

4. Does “normal” apply to:

- total volume only,
- each event type separately,
- or both?

5. When comparing across time, should the system convert to the customer’s local timezone or use UTC?
6. For a location with no historical activity, what should the UI show?

- “No baseline” instead of “below normal”?

7. How should we represent outliers in a way that is explainable to non-technical admins?
8. Should the “normality” signal be descriptive only or should it imply a threshold classification (e.g., healthy / below normal / above normal)?
9. Is multi-location visibility mandatory for the MVP, or just a nice-to-have?
10. Should conversion/routing metrics be included in the first slice, or only volume and event counts?
11. Working assumptions

These are assumptions to proceed if product answers are delayed. They are not explicit requirements.

- “Normal” will be interpreted as a simple customer-specific baseline: recent activity compared against the same account’s own historical recent period.
- “Recent activity” will default to the last 7 completed days, compared with the previous 7-day period.
- The dashboard will show:
- account-level totals,
- event-type breakdown,
- location breakdown,
- and a clear “up/down vs baseline” summary.
- Account timezone will be used for date bucket grouping, while raw data remains in UTC.
- A location without historical baseline will display “insufficient data” rather than a misleading quality signal.
- Empty periods will display zero counts, not silently disappear.
- For the first slice, total volume and event-type counts matter more than sophisticated statistical modeling.
- Duration and outcome fields are treated as optional data, not required for all event types.

1. Seed data analysis

Facts from the actual seed data:

- Number of accounts: 20
- Total activity_events rows: 12,626
- Distinct locations: 15
- Event types:
- call_received: 7,780
- lead_created: 3,044
- appointment_set: 1,802
- Date range:
- earliest event: 2026-02-01 10:57:44
- latest event: 2026-07-27 22:20:34

Multi-location examples

- The data is explicitly multi-location:
- account 1 has multiple site labels across a wide spread
- account 12 also spans multiple site labels
- This strongly supports product focus on location-level visibility.

Timezone considerations

- Accounts have six timezone values:
- America/Chicago
- America/Denver
- America/Los_Angeles
- America/New_York
- America/Phoenix
- UTC
- That means date bucketing and “day” logic must be location/account-aware, not just UTC-based presentation.

Nullable or unusual values

- 398 rows have outcome = NULL
- 5,159 rows have duration_seconds = NULL
- This is important because any “call quality” or “conversion” logic must handle missing values without producing misleading averages or denominators.

Data quality issues

- Missing outcome is common enough that it cannot be ignored.
- Missing duration is common for non-call events but also a real issue for call_received rows.
- The dataset appears synthetic but intentionally messy, which is realistic and relevant to aggregation correctness.
- Certain rows appear incomplete or not meaningfully comparable, so the dashboard should avoid overclaiming on metrics that require a complete denominator.

Edge cases that affect aggregation correctness

- Same account may have multiple locations with different behaviors.
- Some locations may have zero or sparse recent activity.
- A full-data “week” may not exist for all accounts on all dates.
- Event counts can produce false “normal” interpretations if compared across unrelated event types without separating them.
- Missing durations or outcomes could distort per-call averages unless filtered carefully.

1. Smallest valuable vertical slice

The smallest good slice for a 4–6 hour take-home is:

- Account-level dashboard summary with:
- date range selector,
- location filter,
- and a baseline comparison.
- Aggregated KPIs:
- total events in selected period,
- total events vs previous period,
- counts by event type,
- location activity table sorted by deviation from baseline.
- Simple explainability:
- “This period is 22% above your recent baseline”
- “Site C is 35% below your account average”
- One default baseline rule:
- compare the last 7 days to the previous 7 days for the same account
- separate by event type
- and show location-level variance

This is the most valuable slice because it is:

- correct,
- explainable,
- useful to a customer admin,
- and testable without unnecessary feature depth.

I would not start with forecasting, anomaly models, or heavy statistical layering.

1. Aggregation risks

These are the major technical traps that can produce misleading results:

- Timezone handling
- Comparing UTC buckets directly to customer-local “days” will be wrong.
- Date boundaries must respect account timezone.
- Date boundaries
- Rolling week logic should be clear:
- last 7 days,
- last completed week,
- or last full calendar week.
- Mixing calendar dates and rolling windows can subtly mislead.
- Missing locations
- If a location has no events in the selected period, it should not silently vanish from the dashboard.
- It needs either a visible zero or “no data”.
- Zero activity
- A zero may be a valid business signal, not an error.
- But zero should not be interpreted as “normal” without historical context.
- Division by zero
- If calculating rates or average conversion, denominators must be checked.
- A location with no calls or no leads should not produce invalid values.
- Incomplete periods
- Partial weeks or partial months should be treated carefully.
- The dashboard should clearly indicate if the selected period is incomplete.
- Aggregation across locations or event types
- Summing call_received + lead_created + appointment_set is okay for overall event volume but can hide operational differences.
- Event types should be kept separate in the UI and aggregation logic.
- Missing or null fields
- Duration and outcome are not always available.
- Aggregates must exclude nulls appropriately, not treat them as zero unless the business logic says so.

1. Architecture considerations

For a 4–6 hour take-home, the right architecture is intentionally simple:

- .NET 8 backend API
- Angular frontend SPA
- Relational database with SQL migrations
- Minimal layered structure:
- API controllers
- service layer for aggregation
- repository/query layer
- DTOs
- Prefer straightforward SQL or a simple ORM query for the dashboard, not a sophisticated abstraction layer.
- Keep local-state persistence simple:
- localStorage or URL params for selected date range and location
- Avoid:
- distributed jobs,
- async background workers,
- caching layers,
- event buses,
- microservices,
- ML or forecasting components
- These would be unnecessary complexity for the ticket.

1. Suggested implementation order

High-level sequence, without writing code:

1. Confirm the definition of “normal”

- decide baseline semantics and default windows.

2. Define the dashboard contract

- account-level summary
- event-type breakdown
- location comparison
- date range input
- location selector

3. Write tests around aggregation logic

- current-vs-previous period
- zero activity
- missing duration/outcome
- timezone-appropriate period grouping

4. Build the database migration and data model

- ensure seed data loads and migrations are stable.

5. Implement the backend aggregation endpoint

- real aggregation over the seed data
- return clean DTOs for the frontend
- keep business rules explicit and explainable

6. Implement the Angular dashboard

- one user-controlled selector
- state persistence across reload
- clear summary and drill-down

7. Validate manually against the seed data

- confirm totals and comparisons match expectation
- check edge cases and display states

8. Finalize README and AI log

- document assumptions and trade-offs clearly

Decisions that require human approval

- What exactly “normal” means in product terms
- historical baseline vs prior period vs trailing average vs same-week pattern
- What the default “recent activity” window should be
- 7 days, 14 days, 30 days, or last completed week
- Whether the dashboard is primarily:
- account-level only,
- location-level only,
- or both with a drill-down model
- Whether the normality signal should be:
- total volume only,
- by event type,
- or a combined metric
- Whether date bucketing should use:
- account timezone,
- UTC,
- or a chosen local time model
- Whether a location with no historical baseline should show:
- “insufficient data”,
- zero,
- or “below normal”
- Whether to treat missing duration/outcome as:
- excluded from averages,
- zero,
- or unknown values
- Whether the MVP should prioritize correctness and explainability over richer but more complex baseline logic

These are the product and architecture decisions I’d want approved before writing PLAN.md and moving into implementation.
```

## Human review

### Accepted

- Copilot correctly identified the core product problem: help the customer understand whether recent activity is typical and which location may require attention.
- Timezone handling is a correctness concern. Reporting periods should use the account timezone while stored event timestamps remain UTC.
- Locations with zero recent activity must remain visible instead of disappearing from aggregation results.
- Locations with no historical baseline require an explicit state rather than a misleading percentage.
- Event types should remain separate when determining normality.
- The solution should remain intentionally small and avoid unnecessary infrastructure or statistical complexity.

### Rejected / redirected

#### 1. Recent-period definition

Copilot proposed comparing the last 7 completed days with the previous 7-day period.

I rejected that approach because the product context specifically describes a customer admin reviewing the dashboard on Monday morning. A completed local calendar week is easier to explain, validate, and use operationally than an arbitrary rolling 7-day window.

**Approved decision:** the recent period is the last completed local calendar week, Monday 00:00 through the following Monday 00:00, in the account's configured timezone.

#### 2. Baseline definition

Copilot proposed using only the immediately previous 7-day period as the baseline.

I redirected this to the average of the previous four completed calendar weeks. A single prior week may itself be unusually high or low. A four-week average remains simple and explainable while producing a more stable comparison point.

**Approved decision:** baseline = average weekly event count across the previous four completed calendar weeks.

#### 3. Frontend scope

Copilot proposed a date-range selector and location filter.

I reduced the MVP scope. A custom date range introduces additional comparison semantics that are unnecessary for the core ticket. A location filter could also hide the locations the dashboard is intended to surface.

**Approved decision:** the only user-controlled input in the MVP is event type (Calls, Leads, Appointments), persisted through URL query parameters.

#### 4. Aggregation grain

Normality will not combine calls, leads, and appointments into one score.

**Approved decision:** comparison grain is account + location + event type, measured against the location's own recent history.

#### 5. Zero historical baseline

If the historical weekly average is zero:

- `percentageChange = null`
- `status = NoBaseline`

The UI should display a clear "No historical baseline" state rather than `0%` or an infinite percentage.

#### 6. Optional metadata

The seed contains nullable `duration_seconds` and `outcome` values.

Those fields are intentionally not used by this feature because the selected KPI is event count. Missing optional metadata therefore does not invalidate an otherwise valid activity event.

### Approved MVP classification

The first implementation will use a simple, explainable heuristic:

- `BelowTypical`: percentage change < -20%
- `Typical`: percentage change from -20% through +20%
- `AboveTypical`: percentage change > +20%
- `NoBaseline`: historical weekly average = 0

The +/-20% threshold is an MVP product assumption, not a statistically derived anomaly threshold. This limitation must be documented in `PLAN.md` and `README.md`.

### Scope-control decision

The challenge explicitly prioritizes correctness and product judgment over breadth. I therefore chose to spend the limited 4–6 hour budget on:

- correct local-calendar period boundaries,
- correct aggregation,
- multi-location visibility,
- meaningful edge-case tests,
- clear documentation,
- and verification of AI-generated code.

I intentionally deferred custom date ranges, location filtering, charts, forecasting, alerting, advanced statistics, and additional infrastructure.


## Reflection

This interaction was useful because the AI surfaced several real correctness risks before implementation, particularly timezone boundaries, zero-activity locations, and missing historical baselines.

I did not accept the proposed solution wholesale. I changed both the time-window semantics and the baseline model based on the product wording and the Monday-morning operational use case. I also reduced the proposed frontend scope to protect the 4–6 hour timebox.

### Division of labor

- **GitHub Copilot:** analysis assistance, bounded implementation tasks, test generation, and code-review support.
- **Human engineer:** product interpretation, architecture decisions, scope control, acceptance/rejection of agent output, correctness verification, and final ownership.

