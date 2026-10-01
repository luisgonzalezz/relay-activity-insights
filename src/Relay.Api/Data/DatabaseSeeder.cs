using Microsoft.EntityFrameworkCore;

namespace Relay.Api.Data;

public static class DatabaseSeeder
{
    public static void EnsureSeeded(RelayDbContext context, string contentRootPath)
    {
        if (context.Accounts.Any() || context.ActivityEvents.Any())
        {
            return;
        }

        var seedSqlPath = ResolveSeedSqlPath(contentRootPath);
        var seedSql = File.ReadAllText(seedSqlPath);
        context.Database.ExecuteSqlRaw(seedSql);
    }

    private static string ResolveSeedSqlPath(string contentRootPath)
    {
        var candidatePaths = new[]
        {
            Path.Combine(contentRootPath, "data", "seed.sql"),
            Path.Combine(contentRootPath, "..", "..", "data", "seed.sql"),
            Path.Combine(contentRootPath, "..", "..", "..", "data", "seed.sql"),
            Path.Combine(contentRootPath, "..", "..", "..", "..", "data", "seed.sql"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data", "seed.sql")
        };

        foreach (var candidate in candidatePaths)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        throw new FileNotFoundException("Could not locate the seed SQL file for the Relay dataset.", "seed.sql");
    }
}
