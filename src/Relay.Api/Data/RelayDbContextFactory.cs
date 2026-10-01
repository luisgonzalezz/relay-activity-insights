using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Relay.Api.Data;

public class RelayDbContextFactory : IDesignTimeDbContextFactory<RelayDbContext>
{
    public RelayDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RelayDbContext>();
        optionsBuilder.UseSqlite("Data Source=relay.db");

        return new RelayDbContext(optionsBuilder.Options);
    }
}
