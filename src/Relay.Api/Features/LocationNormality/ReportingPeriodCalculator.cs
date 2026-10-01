namespace Relay.Api.Features.LocationNormality;

public sealed record UtcPeriod(
    DateOnly StartDate,
    DateOnly EndDateExclusive,
    DateTime StartUtc,
    DateTime EndUtc);

public sealed record ReportingPeriods(UtcPeriod Current, UtcPeriod Baseline);

public static class ReportingPeriodCalculator
{
    public static ReportingPeriods Calculate(TimeZoneInfo timeZone, DateTime latestActivityUtc)
    {
        var utcAnchor = DateTime.SpecifyKind(latestActivityUtc, DateTimeKind.Utc);
        var localAnchor = TimeZoneInfo.ConvertTimeFromUtc(utcAnchor, timeZone);
        var localDate = new DateTime(localAnchor.Year, localAnchor.Month, localAnchor.Day, 0, 0, 0, DateTimeKind.Unspecified);
        var daysSinceMonday = ((int)localDate.DayOfWeek + 6) % 7;
        var currentEndLocal = localDate.AddDays(-daysSinceMonday);
        var currentStartLocal = currentEndLocal.AddDays(-7);
        var baselineStartLocal = currentStartLocal.AddDays(-28);

        return new ReportingPeriods(
            CreatePeriod(currentStartLocal, currentEndLocal, timeZone),
            CreatePeriod(baselineStartLocal, currentStartLocal, timeZone));
    }

    private static UtcPeriod CreatePeriod(DateTime startLocal, DateTime endLocal, TimeZoneInfo timeZone)
    {
        return new UtcPeriod(
            DateOnly.FromDateTime(startLocal),
            DateOnly.FromDateTime(endLocal),
            TimeZoneInfo.ConvertTimeToUtc(startLocal, timeZone),
            TimeZoneInfo.ConvertTimeToUtc(endLocal, timeZone));
    }
}
