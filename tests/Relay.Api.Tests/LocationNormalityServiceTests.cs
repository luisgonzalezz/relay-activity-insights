using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Relay.Api.Data;
using Relay.Api.Features.LocationNormality;

namespace Relay.Api.Tests;

public class LocationNormalityServiceTests
{
    [Fact]
    public async Task GetAsync_IncludesStartBoundaryAndExcludesEndBoundary()
    {
        using var database = new TestDatabase();
        database.AddAccount("UTC");
        database.AddEvent(Utc("2026-07-27T00:00:00"), "Site A", "call_received");
        database.AddEvent(Utc("2026-08-03T00:00:00"), "Site A", "call_received");
        database.AddEvent(Utc("2026-08-07T12:00:00"), "Site A", "lead_created");
        await database.SaveAsync();

        var response = await database.Service.GetAsync(1, "calls");

        Assert.NotNull(response);
        Assert.Equal(1, response.Locations.Single().CurrentPeriodCount);
    }

    [Fact]
    public async Task GetAsync_DividesBaselineByFourAndKeepsHistoricalLocationAtZeroCurrent()
    {
        using var database = new TestDatabase();
        database.AddAccount("UTC");
        database.AddEventsForWeek(new DateOnly(2026, 6, 30), "Historical Site", "call_received", 20);
        database.AddEventsForWeek(new DateOnly(2026, 7, 14), "Historical Site", "call_received", 12);
        database.AddEventsForWeek(new DateOnly(2026, 7, 21), "Historical Site", "call_received", 8);
        database.AddEvent(Utc("2026-08-07T12:00:00"), "Anchor", "lead_created");
        await database.SaveAsync();

        var response = await database.Service.GetAsync(1, "calls");

        Assert.NotNull(response);
        var historicalSite = Assert.Single(response.Locations);
        Assert.Equal("Historical Site", historicalSite.Location);
        Assert.Equal(0, historicalSite.CurrentPeriodCount);
        Assert.Equal(10m, historicalSite.HistoricalAverage);
        Assert.Equal(-100m, historicalSite.PercentageChange);
        Assert.Equal("BelowTypical", historicalSite.Status);
        Assert.Equal(0, response.Summary.CurrentPeriodCount);
        Assert.Equal(10m, response.Summary.HistoricalAverage);
    }

    [Fact]
    public async Task GetAsync_ReturnsNoBaselineForLocationWithOnlyCurrentActivity()
    {
        using var database = new TestDatabase();
        database.AddAccount("UTC");
        database.AddEvent(Utc("2026-07-28T12:00:00"), "New Site", "call_received");
        database.AddEvent(Utc("2026-08-07T12:00:00"), "Anchor", "lead_created");
        await database.SaveAsync();

        var response = await database.Service.GetAsync(1, "calls");

        Assert.NotNull(response);
        var row = Assert.Single(response.Locations);
        Assert.Equal(1, row.CurrentPeriodCount);
        Assert.Equal(0m, row.HistoricalAverage);
        Assert.Null(row.PercentageChange);
        Assert.Equal("NoBaseline", row.Status);
    }

    [Fact]
    public async Task GetAsync_ClassifiesExactAndOutsideThresholdValues()
    {
        using var database = new TestDatabase();
        database.AddAccount("UTC");

        var baselineWeeks = new[]
        {
            new DateOnly(2026, 6, 29),
            new DateOnly(2026, 7, 6),
            new DateOnly(2026, 7, 13),
            new DateOnly(2026, 7, 20)
        };
        foreach (var week in baselineWeeks)
        {
            database.AddEventsForWeek(week.AddDays(1), "Minus Twenty", "call_received", 5);
            database.AddEventsForWeek(week.AddDays(1), "Below Twenty", "call_received", 5);
            database.AddEventsForWeek(week.AddDays(1), "Plus Twenty", "call_received", 5);
            database.AddEventsForWeek(week.AddDays(1), "Above Twenty", "call_received", 5);
        }

        database.AddEventsForWeek(new DateOnly(2026, 7, 28), "Minus Twenty", "call_received", 4);
        database.AddEventsForWeek(new DateOnly(2026, 7, 28), "Below Twenty", "call_received", 3);
        database.AddEventsForWeek(new DateOnly(2026, 7, 28), "Plus Twenty", "call_received", 6);
        database.AddEventsForWeek(new DateOnly(2026, 7, 28), "Above Twenty", "call_received", 7);
        database.AddEvent(Utc("2026-08-07T12:00:00"), "Anchor", "lead_created");
        await database.SaveAsync();

        var response = await database.Service.GetAsync(1, "calls");

        Assert.NotNull(response);
        var rows = response.Locations.ToDictionary(row => row.Location);
        Assert.Equal("Typical", rows["Minus Twenty"].Status);
        Assert.Equal(-20m, rows["Minus Twenty"].PercentageChange);
        Assert.Equal("BelowTypical", rows["Below Twenty"].Status);
        Assert.True(rows["Below Twenty"].PercentageChange < -20m);
        Assert.Equal("Typical", rows["Plus Twenty"].Status);
        Assert.Equal(20m, rows["Plus Twenty"].PercentageChange);
        Assert.Equal("AboveTypical", rows["Above Twenty"].Status);
        Assert.True(rows["Above Twenty"].PercentageChange > 20m);
    }

    [Fact]
    public async Task GetAsync_FiltersBySelectedEventTypeAndCountsNullableMetrics()
    {
        using var database = new TestDatabase();
        database.AddAccount("UTC");
        database.AddEvent(Utc("2026-07-28T12:00:00"), "Site A", "call_received", null, null);
        database.AddEvent(Utc("2026-07-29T12:00:00"), "Site A", "lead_created");
        database.AddEvent(Utc("2026-08-07T12:00:00"), "Anchor", "appointment_set");
        await database.SaveAsync();

        var calls = await database.Service.GetAsync(1, "calls");
        var leads = await database.Service.GetAsync(1, "leads");

        Assert.NotNull(calls);
        Assert.Equal("calls", calls.EventType);
        Assert.Equal(1, calls.Summary.CurrentPeriodCount);
        Assert.Equal("Site A", Assert.Single(calls.Locations).Location);
        Assert.NotNull(leads);
        Assert.Equal(1, leads.Summary.CurrentPeriodCount);
    }

    [Fact]
    public async Task GetAsync_UsesIndependentBaselinesForEachLocation()
    {
        using var database = new TestDatabase();
        database.AddAccount("UTC");
        var weeks = new[]
        {
            new DateOnly(2026, 6, 29),
            new DateOnly(2026, 7, 6),
            new DateOnly(2026, 7, 13),
            new DateOnly(2026, 7, 20)
        };
        foreach (var week in weeks)
        {
            database.AddEventsForWeek(week.AddDays(1), "Site A", "call_received", 2);
            database.AddEventsForWeek(week.AddDays(1), "Site B", "call_received", 4);
        }

        database.AddEventsForWeek(new DateOnly(2026, 7, 28), "Site A", "call_received", 2);
        database.AddEvent(Utc("2026-08-07T12:00:00"), "Anchor", "lead_created");
        await database.SaveAsync();

        var response = await database.Service.GetAsync(1, "calls");

        Assert.NotNull(response);
        var rows = response.Locations.ToDictionary(row => row.Location);
        Assert.Equal(2m, rows["Site A"].HistoricalAverage);
        Assert.Equal(0m, rows["Site A"].PercentageChange);
        Assert.Equal(4m, rows["Site B"].HistoricalAverage);
        Assert.Equal(0, rows["Site B"].CurrentPeriodCount);
        Assert.Equal(-100m, rows["Site B"].PercentageChange);
    }

    [Fact]
    public async Task GetAsync_UsesAccountTimezoneForPeriodsAndAllEventTypesForAnchor()
    {
        using var database = new TestDatabase();
        database.AddAccount("America/Chicago");
        database.AddEvent(Utc("2026-07-27T06:00:00"), "Site A", "call_received");
        database.AddEvent(Utc("2026-08-07T12:00:00"), "Anchor", "lead_created");
        await database.SaveAsync();

        var response = await database.Service.GetAsync(1, "calls");

        Assert.NotNull(response);
        Assert.Equal(new DateOnly(2026, 7, 27), response.CurrentPeriod.StartDate);
        Assert.Equal(new DateOnly(2026, 8, 3), response.CurrentPeriod.EndDateExclusive);
        Assert.Equal(1, response.Summary.CurrentPeriodCount);
    }

    [Fact]
    public async Task GetAsync_ReturnsNullForUnknownAccount()
    {
        using var database = new TestDatabase();
        var response = await database.Service.GetAsync(404, "calls");

        Assert.Null(response);
    }

    private static DateTime Utc(string value) =>
        DateTime.SpecifyKind(
            DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.None),
            DateTimeKind.Utc);

    internal sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        private int _nextEventId = 1;

        public TestDatabase()
        {
            _connection.Open();
            var options = new DbContextOptionsBuilder<RelayDbContext>()
                .UseSqlite(_connection)
                .Options;
            Context = new RelayDbContext(options);
            Context.Database.Migrate();
            Service = new LocationNormalityService(Context);
        }

        public RelayDbContext Context { get; }
        public LocationNormalityService Service { get; }

        public void AddAccount(string timeZone)
        {
            Context.Accounts.Add(new Account
            {
                Id = 1,
                Name = "Test Account",
                Industry = "Testing",
                Timezone = timeZone,
                CreatedAt = Utc("2026-01-01T00:00:00")
            });
        }

        public void AddEvent(
            DateTime occurredAt,
            string location,
            string eventType,
            int? durationSeconds = 60,
            string? outcome = "connected")
        {
            Context.ActivityEvents.Add(new ActivityEvent
            {
                Id = _nextEventId++,
                AccountId = 1,
                Location = location,
                EventType = eventType,
                OccurredAt = occurredAt,
                DurationSeconds = durationSeconds,
                Outcome = outcome
            });
        }

        public void AddEventsForWeek(DateOnly eventDate, string location, string eventType, int count)
        {
            var occurredAt = eventDate.ToDateTime(new TimeOnly(12, 0), DateTimeKind.Utc);
            for (var index = 0; index < count; index++)
            {
                AddEvent(occurredAt, location, eventType);
            }
        }

        public Task<int> SaveAsync() => Context.SaveChangesAsync();

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }
}
