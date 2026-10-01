import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  ActivityEventType,
  LocationNormalityResponse
} from './location-normality.models';

@Injectable({ providedIn: 'root' })
export class LocationNormalityApiService {
  private readonly http = inject(HttpClient);

  get(accountId: number, eventType: ActivityEventType): Observable<LocationNormalityResponse> {
    const params = new HttpParams()
      .set('accountId', accountId)
      .set('eventType', eventType);

    return this.http.get<LocationNormalityResponse>('/api/location-normality', { params });
  }
}
