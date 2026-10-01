namespace Relay.Api.Features.LocationNormality;

public sealed record LocationNormalityPeriod(
    DateOnly StartDate,
    DateOnly EndDateExclusive);

public sealed record ActivitySummary(
    int CurrentPeriodCount,
    decimal HistoricalAverage,
    decimal? PercentageChange,
    string Status);

public sealed record LocationNormalityRow(
    string Location,
    int CurrentPeriodCount,
    decimal HistoricalAverage,
    decimal? PercentageChange,
    string Status);

public sealed record LocationNormalityResponse(
    int AccountId,
    string AccountName,
    string AccountTimezone,
    string EventType,
    LocationNormalityPeriod CurrentPeriod,
    LocationNormalityPeriod BaselinePeriod,
    ActivitySummary Summary,
    IReadOnlyList<LocationNormalityRow> Locations);
