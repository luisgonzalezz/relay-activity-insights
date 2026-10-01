import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { DashboardComponent } from './dashboard.component';
import { LocationNormalityResponse } from './location-normality.models';

describe('DashboardComponent', () => {
  let http: HttpTestingController;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        provideRouter([{ path: 'dashboard', component: DashboardComponent }]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  afterEach(() => http.verify());

  it('loads the event type from the query parameter and requests matching data', async () => {
    const harness = await RouterTestingHarness.create();
    const component = await harness.navigateByUrl('/dashboard?eventType=leads', DashboardComponent);

    expect(component.eventType()).toBe('leads');
    const request = http.expectOne(req =>
      req.url === '/api/location-normality' &&
      req.params.get('accountId') === '1' &&
      req.params.get('eventType') === 'leads'
    );
    request.flush(response('leads'));
    harness.detectChanges();

    expect(harness.routeNativeElement?.textContent).toContain('Summit Auto Group');
  });

  it('defaults a missing event type to calls and writes it to the URL', async () => {
    const harness = await RouterTestingHarness.create();
    const component = await harness.navigateByUrl('/dashboard', DashboardComponent);
    http.expectOne(req => req.params.get('eventType') === 'calls').flush(response('calls'));
    await harness.fixture.whenStable();
    harness.detectChanges();

    expect(component.eventType()).toBe('calls');
    expect(router.url).toContain('eventType=calls');
  });

  it('updates the query parameter and requests data when the selector changes', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/dashboard?eventType=calls', DashboardComponent);
    http.expectOne(req => req.params.get('eventType') === 'calls').flush(response('calls'));

    const select = harness.routeNativeElement?.querySelector('select');
    expect(select).not.toBeNull();
    (select as HTMLSelectElement).value = 'appointments';
    select?.dispatchEvent(new Event('change'));
    await harness.fixture.whenStable();

    expect(router.url).toContain('eventType=appointments');
    http.expectOne(req =>
      req.url === '/api/location-normality' &&
      req.params.get('accountId') === '1' &&
      req.params.get('eventType') === 'appointments'
    ).flush(response('appointments'));
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('Activity summary');
  });

  it('preserves a valid selected type on initialization after a page reload', async () => {
    const harness = await RouterTestingHarness.create();
    const component = await harness.navigateByUrl('/dashboard?eventType=leads', DashboardComponent);
    http.expectOne(req => req.params.get('eventType') === 'leads').flush(response('leads'));
    harness.detectChanges();

    expect(component.eventType()).toBe('leads');
    expect(router.url).toBe('/dashboard?eventType=leads');
  });

  it('renders backend statuses safely, including a null percentage for NoBaseline', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/dashboard?eventType=calls', DashboardComponent);
    const data = response('calls');
    data.summary = {
      currentPeriodCount: 1,
      historicalAverage: 0,
      percentageChange: null,
      status: 'NoBaseline'
    };
    data.locations = [
      {
        location: 'New Location',
        currentPeriodCount: 1,
        historicalAverage: 0,
        percentageChange: null,
        status: 'NoBaseline'
      },
      {
        location: 'Typical Location',
        currentPeriodCount: 5,
        historicalAverage: 5,
        percentageChange: 0,
        status: 'Typical'
      },
      {
        location: 'Below Location',
        currentPeriodCount: 3,
        historicalAverage: 5,
        percentageChange: -40,
        status: 'BelowTypical'
      },
      {
        location: 'Above Location',
        currentPeriodCount: 7,
        historicalAverage: 5,
        percentageChange: 40,
        status: 'AboveTypical'
      }
    ];
    http.expectOne(req => req.params.get('eventType') === 'calls').flush(data);
    harness.detectChanges();

    const page = harness.routeNativeElement?.textContent ?? '';
    expect(page).toContain('No baseline');
    expect(page).toContain('Below typical');
    expect(page).toContain('Typical');
    expect(page).toContain('Above typical');
    expect(page).toContain('—');
    expect(page).not.toContain('NaN');
    expect(page).not.toContain('Infinity');

    const rows = Array.from(harness.routeNativeElement?.querySelectorAll('tbody tr') ?? []);
    expect(rows.map(row => row.textContent?.trim().split(/\s+/)[0])).toEqual([
      'Below', 'Typical', 'Above', 'New'
    ]);
  });

  it('defaults invalid query parameter values to calls', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/dashboard?eventType=messages', DashboardComponent);
    http.expectOne(req => req.params.get('eventType') === 'calls').flush(response('calls'));
    await harness.fixture.whenStable();
    harness.detectChanges();

    expect(router.url).toContain('eventType=calls');
  });
});

function response(eventType: 'calls' | 'leads' | 'appointments'): LocationNormalityResponse {
  return {
    accountId: 1,
    accountName: 'Summit Auto Group',
    accountTimezone: 'America/Chicago',
    eventType,
    currentPeriod: {
      startDate: '2026-07-20',
      endDateExclusive: '2026-07-27'
    },
    baselinePeriod: {
      startDate: '2026-06-22',
      endDateExclusive: '2026-07-20'
    },
    summary: {
      currentPeriodCount: 34,
      historicalAverage: 28.5,
      percentageChange: 19.298245614,
      status: 'Typical'
    },
    locations: []
  };
}
