using Microsoft.EntityFrameworkCore;
using Relay.Api.Data;

namespace Relay.Api.Tests;

public class DatabaseFoundationTests
{
    [Fact]
    public void MigrationsAndSeed_LoadExpectedDataset()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"relay-foundation-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={dbPath}";

        var options = new DbContextOptionsBuilder<RelayDbContext>()
            .UseSqlite(connectionString)
            .Options;

        using var context = new RelayDbContext(options);
        context.Database.Migrate();

        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        DatabaseSeeder.EnsureSeeded(context, repoRoot);

        Assert.Equal(20, context.Accounts.Count());
        Assert.Equal(12626, context.ActivityEvents.Count());
    }
}
