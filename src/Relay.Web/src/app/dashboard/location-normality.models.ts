export type ActivityEventType = 'calls' | 'leads' | 'appointments';

export type NormalityStatus =
  | 'BelowTypical'
  | 'Typical'
  | 'AboveTypical'
  | 'NoBaseline';

export interface LocationNormalityPeriod {
  startDate: string;
  endDateExclusive: string;
}

export interface ActivitySummary {
  currentPeriodCount: number;
  historicalAverage: number;
  percentageChange: number | null;
  status: NormalityStatus;
}

export interface LocationNormalityRow {
  location: string;
  currentPeriodCount: number;
  historicalAverage: number;
  percentageChange: number | null;
  status: NormalityStatus;
}

export interface LocationNormalityResponse {
  accountId: number;
  accountName: string;
  accountTimezone: string;
  eventType: ActivityEventType;
  currentPeriod: LocationNormalityPeriod;
  baselinePeriod: LocationNormalityPeriod;
  summary: ActivitySummary;
  locations: LocationNormalityRow[];
}
