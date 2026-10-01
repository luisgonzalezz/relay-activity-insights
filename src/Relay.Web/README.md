# Relay activity dashboard

Small Angular dashboard for the backend location-normality endpoint. It shows account totals and per-location comparisons for calls, leads, or appointments. The selected event type is stored in the `eventType` URL query parameter; no browser storage is used.

## Demo account

The account context is intentionally fixed to account ID `1` (Summit Auto Group) in `src/app/dashboard/dashboard.component.ts`. Replace the `accountId` property when account selection or authentication is introduced.

## Run locally

Start the backend on its configured HTTP URL (`http://localhost:5126`), then run:

```bash
npm install
npm start
```

Open `http://localhost:4200/dashboard?eventType=calls`. The Angular development server proxies `/api` to `http://localhost:5126` using `proxy.conf.json`. Select Leads or Appointments to update the URL and load that event type; reloading the page preserves the selected type.

## Build and tests

```bash
npm run build
npx ng test --watch=false --browsers=ChromeHeadless --progress=false
```

The focused component tests cover query-parameter initialization and updates, matching API requests, selection persistence on initialization, and safe status/NoBaseline rendering.
