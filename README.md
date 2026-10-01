# Relay Activity Insights

## Overview

Relay helps service businesses track calls, leads, and appointments across multiple locations. DASH-247 addresses a common question customer admins cannot answer from raw totals: “Is this activity normal for us?”

This MVP compares the latest completed week with each location’s own recent history. It provides an account summary and a location comparison table for one selected event type.

## Interpretation of DASH-247

- Recent activity is the most recent completed local calendar week, Monday 00:00 through the following Monday 00:00.
- The baseline is the average event count across the previous four completed local calendar weeks. Weeks with zero events remain part of the four-week denominator.
- Comparisons are made per account, location, and selected event type. Calls, leads, and appointments are not combined.
- A percentage change below -20% is BelowTypical; -20% through +20% is Typical; above +20% is AboveTypical.
- The +/-20% threshold is an MVP product heuristic, not a statistically derived anomaly model.
- If the historical average is zero, percentage change is `null` and status is `NoBaseline`.

## Architecture

**Backend:** .NET 8, ASP.NET Core minimal APIs, EF Core, and SQLite. EF Core migrations create the schema, and EF Core queries read the account and activity data. The API exposes `GET /api/location-normality?accountId={id}&eventType={calls|leads|appointments}`.

**Frontend:** Angular 21.2 standalone components, `HttpClient`, and Angular Router query parameters. A feature-local service consumes the API response; the selected event type is persisted in the URL, not browser storage.

The implementation deliberately uses one API project, one focused feature, direct EF Core access, and one dashboard view. It avoids additional architecture and infrastructure that would not help this take-home slice.

## Data and reporting semantics

- `activity_events.occurred_at` values are stored as UTC.
- The account’s configured timezone defines the reporting calendar.
- Local week boundaries are converted to UTC before events are queried.
- Activity windows use half-open intervals: `[startUtc, endUtc)`.
- The challenge seed data is historical. The latest available event timestamp for the selected account, across all event types, is used as the deterministic reporting anchor so the dashboard remains populated regardless of the machine clock.
- A production system would normally anchor reporting to the current clock and handle data freshness explicitly. The seed-data anchor was discovered after `PLAN.md` was frozen and is documented here without retroactively changing that plan.

## Location semantics

There is no locations table. For the selected account and event type, the comparison location set is the union of locations observed during the current comparison week or any of the four baseline weeks. Thus, a location with baseline activity and zero current activity remains visible. Locations with no events in any of those periods cannot be inferred from this schema.

## Database setup

Requirements: .NET 8 SDK. From the repository root, restore the solution:

```sh
dotnet restore Relay.ActivityInsights.slnx
```

Install the EF Core CLI once if it is not already available:

```sh
dotnet tool install --global dotnet-ef --version 8.0.11
```

Apply the initial migration from the API project directory:

```sh
cd src/Relay.Api
dotnet ef database update --project Relay.Api.csproj --startup-project Relay.Api.csproj
```

The schema is created by the migration, not `EnsureCreated()`. Starting the API loads the provided `data/seed.sql` into an empty database. From the same `src/Relay.Api` directory, run:

```sh
dotnet run --no-launch-profile --urls http://localhost:5126
```

The seed contains **20 accounts** and **12,626 activity_events**. `DatabaseFoundationTests` verifies those counts. The SQLite database is generated locally and is not part of the repository.

## Backend run instructions

After database setup, run the API from `src/Relay.Api`:

```sh
dotnet run --no-launch-profile --urls http://localhost:5126
```

The API applies pending migrations and loads the seed on startup. Health check: `http://localhost:5126/health`. In Development, Swagger is available at `/swagger`.

## Frontend run instructions

Requirements: Node.js 22.12+ (or 20.19+) and npm. In a separate terminal, from the repository root:

```sh
cd src/Relay.Web
npm ci
npm start
```

Open `http://localhost:4200/dashboard?eventType=calls`. The Angular development server proxies `/api` to `http://localhost:5126` as configured in `src/Relay.Web/proxy.conf.json`; start the backend first. Calls is selected by default when `eventType` is missing or invalid. Changing the selector updates the URL and requests the corresponding event type.

## Tests

From the repository root, run the backend tests:

```sh
dotnet test tests/Relay.Api.Tests/Relay.Api.Tests.csproj
```

Run the Angular tests from `src/Relay.Web` (Chrome/Chrome Headless must be installed):

```sh
npx ng test --watch=false --browsers=ChromeHeadless --progress=false
```

At final verification, **15 backend tests** and **6 Angular tests** passed. The backend tests cover reporting periods, timezone/DST boundaries, half-open intervals, baseline denominators, event filtering, status thresholds, and account/error cases. The Angular tests cover query parameter state, API requests, persistence on initialization, and status rendering.

## Demo

The demo uses fixed **accountId 1**, **Summit Auto Group**, and defaults to **Calls**. The verified Calls result is:

- Current total: **34**
- Four-week average: **28.5**
- Change: approximately **+19.3%**
- Status: **Typical**

Leads and Appointments were also manually verified through the real API and dashboard.

## AI-assisted development workflow

GitHub Copilot was used for requirements analysis, planning assistance, bounded implementation, test generation, and review support. Human review controlled product interpretation, architecture, scope, acceptance or rejection of generated work, independent aggregate verification, and final ownership. AI output was not accepted automatically.

See [`PLAN.md`](PLAN.md) for the pre-implementation plan and [`ai-log/`](ai-log/) for the interaction history and concrete corrections made during implementation.

## Key AI corrections / review moments

1. An initial suggestion to compare the last seven days with the previous seven days was rejected in favor of completed account-local calendar weeks and a four-week baseline.
2. The initial EF migration generated PascalCase columns that did not match the canonical snake_case SQL schema and seed; the migration mapping was corrected.
3. A test helper parsed UTC fixture strings through machine-local time; it was corrected to preserve UTC.
4. A Sunday reporting anchor was initially expected to select the week that had just ended. That expectation was corrected to the last fully completed Monday-to-Monday week.

## Trade-offs

- SQLite was chosen over SQL Server for reviewer portability and the timebox, although the production team uses SQL Server.
- The MVP has no charts, custom date range, or location filter.
- A fixed demo account is used because authentication and account selection are out of scope.
- The explainable +/-20% heuristic was chosen instead of statistical anomaly detection.
- `npm audit --omit=dev` reported zero known vulnerabilities in production dependencies at final verification. Five moderate findings remain in the Angular development-server dependency chain; the available fix requires a breaking upgrade to Angular CLI 22. This is a tooling-only finding, not a guarantee that the application is vulnerability-free, and the major toolchain upgrade was deferred as outside this challenge’s scope.

## Deliberately deferred

- Authentication and account selection
- Custom date ranges and location filtering
- Charts
- Alerting and notifications
- ML and forecasting
- SQL Server setup and production deployment infrastructure
- CI/CD, Docker, caching, background jobs, and additional architecture layers

## What I would do with another day

- Validate the +/-20% threshold with Product and real customer behavior.
- Introduce an explicit locations dimension if the product domain can provide locations with no activity in the sampled periods.
- Add more end-to-end/browser automation and improve accessibility.
- Add data freshness UX and API observability.
- Evaluate provider-specific query and timezone behavior against SQL Server.
