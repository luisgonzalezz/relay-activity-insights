using System.Globalization;
using Relay.Api.Features.LocationNormality;

namespace Relay.Api.Tests;

public class ReportingPeriodCalculatorTests
{
    [Fact]
    public void Calculate_UsesMostRecentCompletedMondayToMondayWeek()
    {
        var periods = ReportingPeriodCalculator.Calculate(
            TimeZoneInfo.Utc,
            Utc("2026-07-28T16:00:00"));

        Assert.Equal(new DateOnly(2026, 7, 20), periods.Current.StartDate);
        Assert.Equal(new DateOnly(2026, 7, 27), periods.Current.EndDateExclusive);
        Assert.Equal(new DateOnly(2026, 6, 22), periods.Baseline.StartDate);
        Assert.Equal(new DateOnly(2026, 7, 20), periods.Baseline.EndDateExclusive);
    }

    [Fact]
    public void Calculate_ConvertsLocalWeekBoundariesToUtcAcrossDst()
    {
        var chicago = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");
        var periods = ReportingPeriodCalculator.Calculate(
            chicago,
            Utc("2026-03-11T15:00:00"));

        Assert.Equal(new DateOnly(2026, 3, 2), periods.Current.StartDate);
        Assert.Equal(new DateOnly(2026, 3, 9), periods.Current.EndDateExclusive);
        Assert.Equal(Utc("2026-03-02T06:00:00"), periods.Current.StartUtc);
        Assert.Equal(Utc("2026-03-09T05:00:00"), periods.Current.EndUtc);
        Assert.Equal(TimeSpan.FromHours(167), periods.Current.EndUtc - periods.Current.StartUtc);
    }

    [Fact]
    public void Calculate_InterpretsLatestActivityInAccountTimezone()
    {
        var chicago = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");
        var periods = ReportingPeriodCalculator.Calculate(
            chicago,
            Utc("2026-08-03T04:30:00"));

        Assert.Equal(new DateOnly(2026, 7, 20), periods.Current.StartDate);
        Assert.Equal(new DateOnly(2026, 7, 27), periods.Current.EndDateExclusive);
        Assert.Equal(Utc("2026-07-20T05:00:00"), periods.Current.StartUtc);
        Assert.Equal(Utc("2026-07-27T05:00:00"), periods.Current.EndUtc);
    }

    private static DateTime Utc(string value) =>
        DateTime.SpecifyKind(
            DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.None),
            DateTimeKind.Utc);
}
