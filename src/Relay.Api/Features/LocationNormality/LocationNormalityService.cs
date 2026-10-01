using Microsoft.EntityFrameworkCore;
using Relay.Api.Data;

namespace Relay.Api.Features.LocationNormality;

public sealed class LocationNormalityService(RelayDbContext dbContext)
{
    public async Task<LocationNormalityResponse?> GetAsync(
        int accountId,
        string eventType,
        CancellationToken cancellationToken = default)
    {
        var normalizedEventType = eventType.ToLowerInvariant();
        var eventTypeValue = normalizedEventType switch
        {
            "calls" => "call_received",
            "leads" => "lead_created",
            "appointments" => "appointment_set",
            _ => throw new UnsupportedEventTypeException(eventType)
        };

        var account = await dbContext.Accounts
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == accountId, cancellationToken);

        if (account is null)
        {
            return null;
        }

        var latestActivityUtc = await dbContext.ActivityEvents
            .Where(activity => activity.AccountId == accountId)
            .Select(activity => (DateTime?)activity.OccurredAt)
            .MaxAsync(cancellationToken);

        if (latestActivityUtc is null)
        {
            throw new AccountHasNoActivityException(accountId);
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(account.Timezone);
        var periods = ReportingPeriodCalculator.Calculate(timeZone, latestActivityUtc.Value);
        var baselineWeeks = Enumerable.Range(0, 4)
            .Select(week =>
            {
                var startLocal = periods.Baseline.StartDate.ToDateTime(TimeOnly.MinValue).AddDays(week * 7);
                var endLocal = startLocal.AddDays(7);
                return new UtcPeriod(
                    DateOnly.FromDateTime(startLocal),
                    DateOnly.FromDateTime(endLocal),
                    TimeZoneInfo.ConvertTimeToUtc(startLocal, timeZone),
                    TimeZoneInfo.ConvertTimeToUtc(endLocal, timeZone));
            })
            .ToArray();

        var activity = await dbContext.ActivityEvents
            .AsNoTracking()
            .Where(candidate =>
                candidate.AccountId == accountId &&
                candidate.EventType == eventTypeValue &&
                candidate.OccurredAt >= periods.Baseline.StartUtc &&
                candidate.OccurredAt < periods.Current.EndUtc)
            .Select(candidate => new { candidate.Location, candidate.OccurredAt })
            .ToListAsync(cancellationToken);

        var countsByLocation = new Dictionary<string, LocationCounts>(StringComparer.Ordinal);
        foreach (var row in activity)
        {
            if (!countsByLocation.TryGetValue(row.Location, out var counts))
            {
                counts = new LocationCounts();
                countsByLocation.Add(row.Location, counts);
            }

            var occurredAtUtc = DateTime.SpecifyKind(row.OccurredAt, DateTimeKind.Utc);
            if (occurredAtUtc >= periods.Current.StartUtc && occurredAtUtc < periods.Current.EndUtc)
            {
                counts.CurrentPeriodCount++;
                continue;
            }

            for (var week = 0; week < baselineWeeks.Length; week++)
            {
                if (occurredAtUtc >= baselineWeeks[week].StartUtc && occurredAtUtc < baselineWeeks[week].EndUtc)
                {
                    counts.BaselineWeeklyCounts[week]++;
                    break;
                }
            }
        }

        var locationRows = countsByLocation
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => CreateRow(pair.Key, pair.Value))
            .ToArray();
        var accountCounts = new LocationCounts
        {
            CurrentPeriodCount = locationRows.Sum(row => row.CurrentPeriodCount)
        };

        for (var week = 0; week < 4; week++)
        {
            accountCounts.BaselineWeeklyCounts[week] = locationRows
                .Sum(row => countsByLocation[row.Location].BaselineWeeklyCounts[week]);
        }

        return new LocationNormalityResponse(
            account.Id,
            account.Name,
            account.Timezone,
            normalizedEventType,
            ToDto(periods.Current),
            ToDto(periods.Baseline),
            CreateSummary(accountCounts),
            locationRows);
    }

    private static LocationNormalityRow CreateRow(string location, LocationCounts counts)
    {
        var summary = CreateSummary(counts);
        return new LocationNormalityRow(
            location,
            summary.CurrentPeriodCount,
            summary.HistoricalAverage,
            summary.PercentageChange,
            summary.Status);
    }

    private static ActivitySummary CreateSummary(LocationCounts counts)
    {
        var historicalAverage = counts.BaselineWeeklyCounts.Sum() / 4m;
        decimal? percentageChange = historicalAverage == 0
            ? null
            : (counts.CurrentPeriodCount - historicalAverage) / historicalAverage * 100m;
        var status = percentageChange switch
        {
            null => "NoBaseline",
            < -20m => "BelowTypical",
            > 20m => "AboveTypical",
            _ => "Typical"
        };

        return new ActivitySummary(
            counts.CurrentPeriodCount,
            historicalAverage,
            percentageChange,
            status);
    }

    private static LocationNormalityPeriod ToDto(UtcPeriod period) =>
        new(period.StartDate, period.EndDateExclusive);

    private sealed class LocationCounts
    {
        public int CurrentPeriodCount { get; set; }
        public int[] BaselineWeeklyCounts { get; } = new int[4];
    }
}

public sealed class AccountHasNoActivityException(int accountId)
    : Exception($"Account {accountId} has no activity to establish a reporting period.");

public sealed class UnsupportedEventTypeException(string eventType)
    : Exception($"Unsupported event type '{eventType}'. Supported values: calls, leads, appointments.");
