import { DecimalPipe, NgClass } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, distinctUntilChanged, map, of, switchMap, tap } from 'rxjs';
import { LocationNormalityApiService } from './location-normality-api.service';
import {
  ActivityEventType,
  LocationNormalityPeriod,
  LocationNormalityResponse,
  LocationNormalityRow,
  NormalityStatus
} from './location-normality.models';

const EVENT_TYPES: readonly ActivityEventType[] = ['calls', 'leads', 'appointments'];
const STATUS_ORDER: Record<NormalityStatus, number> = {
  BelowTypical: 0,
  Typical: 1,
  AboveTypical: 2,
  NoBaseline: 3
};

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [DecimalPipe, NgClass],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(LocationNormalityApiService);

  readonly accountId = 1;
  readonly eventType = signal<ActivityEventType>('calls');
  readonly result = signal<LocationNormalityResponse | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly sortedLocations = computed(() => {
    const locations = this.result()?.locations ?? [];
    return [...locations].sort((left, right) => {
      const statusDifference = STATUS_ORDER[left.status] - STATUS_ORDER[right.status];
      if (statusDifference !== 0) {
        return statusDifference;
      }

      if (left.status === 'BelowTypical') {
        return (left.percentageChange ?? 0) - (right.percentageChange ?? 0);
      }

      return left.location.localeCompare(right.location);
    });
  });

  ngOnInit(): void {
    this.route.queryParamMap.pipe(
      map(params => params.get('eventType')),
      distinctUntilChanged(),
      tap(value => {
        if (!this.isEventType(value)) {
          void this.router.navigate([], {
            relativeTo: this.route,
            queryParams: { eventType: 'calls' },
            queryParamsHandling: 'merge',
            replaceUrl: true
          });
        }
      }),
      map(value => this.isEventType(value) ? value : 'calls'),
      distinctUntilChanged(),
      tap(eventType => {
        this.eventType.set(eventType);
        this.result.set(null);
        this.loading.set(true);
        this.error.set(null);
      }),
      switchMap(eventType =>
        this.api.get(this.accountId, eventType).pipe(
          map(result => ({ result, error: null })),
          catchError((error: HttpErrorResponse) =>
            of({
              result: null,
              error: error.status
                ? `The activity data could not be loaded (HTTP ${error.status}).`
                : 'The activity data could not be loaded. Check that the API is running and try again.'
            })
          )
        )
      )
    ).subscribe(outcome => {
      this.result.set(outcome.result);
      this.error.set(outcome.error);
      this.loading.set(false);
    });
  }

  onEventTypeChange(event: Event): void {
    const selected = (event.target as HTMLSelectElement).value;
    if (!this.isEventType(selected) || selected === this.eventType()) {
      return;
    }

    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { eventType: selected },
      queryParamsHandling: 'merge'
    });
  }

  statusLabel(status: NormalityStatus): string {
    switch (status) {
      case 'BelowTypical': return 'Below typical';
      case 'Typical': return 'Typical';
      case 'AboveTypical': return 'Above typical';
      case 'NoBaseline': return 'No baseline';
    }
  }

  formatPercentage(value: number | null): string {
    if (value === null || !Number.isFinite(value)) {
      return '—';
    }

    const formatted = new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 1,
      maximumFractionDigits: 1
    }).format(Math.abs(value));
    return `${value > 0 ? '+' : value < 0 ? '-' : ''}${formatted}%`;
  }

  formatPeriod(period: LocationNormalityPeriod): string {
    const start = this.toUtcDate(period.startDate);
    const end = this.toUtcDate(period.endDateExclusive);
    end.setUTCDate(end.getUTCDate() - 1);
    return `${this.formatDate(start)} – ${this.formatDate(end)}`;
  }

  statusClass(status: NormalityStatus): string {
    return `status-${status.toLowerCase()}`;
  }

  private isEventType(value: string | null): value is ActivityEventType {
    return value !== null && EVENT_TYPES.includes(value as ActivityEventType);
  }

  private toUtcDate(date: string): Date {
    const [year, month, day] = date.split('-').map(Number);
    return new Date(Date.UTC(year, month - 1, day));
  }

  private formatDate(date: Date): string {
    return new Intl.DateTimeFormat('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      timeZone: 'UTC'
    }).format(date);
  }
}
