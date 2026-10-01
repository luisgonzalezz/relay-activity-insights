# AI Interaction 05 — Frontend Implementation

## Tool

GitHub Copilot

## Purpose

Implement only the Angular MVP defined by the approved plan:

- one dashboard view
- one event-type selector
- URL query-param persistence
- consumption of the existing backend endpoint
- account summary
- location comparison table
- loading/error/empty/NoBaseline states
- focused frontend tests
- real integration verification

No backend redesign, charts, auth, NgRx, or extra dashboard features were allowed.

## Copilot implementation result

```text
Implemented the Angular dashboard slice under `src/Relay.Web`. It consumes the existing backend contract through `HttpClient`, uses account 1 as the documented demo account, and keeps event type in the URL query string. No backend code, `PLAN.md`, or AI logs were modified.

Dashboard behavior:
- Displays account summary
- Uses backend-supplied reporting periods
- Displays all location rows
- Formats percentages without changing API values
- Uses backend statuses for labels and ordering
- Handles loading, errors, empty results, and NoBaseline

Verification:
- Production build succeeded
- 6 Angular tests passed
- Headless browser integration against the real API confirmed:
  - Calls:
    - Summit Auto Group
    - 34 current events
    - 28.5 weekly average
    - +19.3%
    - Typical
  - Leads:
    - 12 current
    - 12.75 weekly average
  - Appointments:
    - 7 current
    - 7.25 weekly average
- Changing event type updated both the URL and the API request
- Refresh preserved each selected event type
- Each event type displayed six locations

Files:
- Created Angular app
- Dashboard component and tests
- Typed models
- API service
- Development proxy
- Frontend README

Dependency decision:
- Initial Angular 19 scaffold had high-severity findings
- Upgraded to Angular 21.2
- Production dependency audit now reports zero vulnerabilities
- Five moderate findings remain in the development-server dependency chain
- Available fix would require breaking Angular CLI 22 upgrade
```

## Human review

### Accepted

- The implementation stayed within the approved frontend-only scope.
- Angular consumes the existing API contract rather than duplicating aggregation logic.
- Account 1 is used as the fixed demo context, consistent with the challenge's out-of-scope authentication/account-selection guidance.
- Event type is persisted through URL query parameters rather than local storage.
- The UI handles loading, API errors, empty location collections, and `NoBaseline`.
- Percentage formatting is presentation-only and does not mutate API values.
- Status labels come from backend status values rather than reimplementing classification thresholds in Angular.
- The frontend was verified against the real seeded backend, not only with mocks.
- Calls, Leads, and Appointments all updated the URL, changed the API request, and survived browser refresh.

### Integration verification

The already-verified backend result for Calls was reproduced in the browser:

- account: Summit Auto Group
- current total: 34
- four-week average: 28.5
- percentage change: +19.3%
- status: Typical

Additional event types also produced stable results:

- Leads: 12 current / 12.75 baseline average
- Appointments: 7 current / 7.25 baseline average

Each event type displayed six locations.

### Dependency decision

The initial Angular 19 scaffold surfaced high-severity dependency findings.

The implementation was upgraded to Angular 21.2, after which the production dependency audit reported zero vulnerabilities.

Five moderate findings remain in the development-server dependency chain. The available automatic fix requires a breaking Angular CLI 22 upgrade.

That upgrade is intentionally deferred because:

- the remaining findings are in development tooling rather than production dependencies,
- Angular 21.2 already removes the high-severity production concern,
- forcing another major CLI upgrade would consume time without improving the evaluated product slice,
- the take-home explicitly rewards scope control and verification over unnecessary breadth.

This should be documented in the top-level README under trade-offs / known limitations.

### AI / implementation review notes

No backend changes were required to accommodate the frontend, which confirms the endpoint contract was sufficiently usable.

No additional feature expansion is approved at this point. In particular, charts, additional filters, global state management, and account selection remain intentionally deferred.

## Decision

The Angular MVP is approved.

The project now satisfies the functional skeleton required by the challenge:

- .NET backend with real aggregation
- Angular SPA
- user-controlled input with reload persistence
- relational database and migrations
- seed-data loading
- backend and frontend tests
- agent-first workflow artifacts

The remaining work should focus on final verification, top-level documentation, and the final AI reflection rather than additional features.

## Reflection

This step demonstrates deliberate scope control.

The frontend did not reinterpret the business rules; it treated the backend as the source of truth for:

- reporting periods
- aggregation
- percentage values
- status classification

The dependency upgrade was also treated as a bounded engineering decision rather than an excuse to broaden the task. Once production dependency risk was addressed, the remaining development-only findings were documented instead of triggering another breaking framework upgrade.
