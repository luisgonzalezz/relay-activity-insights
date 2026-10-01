using Microsoft.EntityFrameworkCore;

namespace Relay.Api.Data;

public class RelayDbContext : DbContext
{
    public RelayDbContext(DbContextOptions<RelayDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<ActivityEvent> ActivityEvents => Set<ActivityEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            entity.Property(e => e.Industry).HasColumnName("industry").HasMaxLength(60).IsRequired();
            entity.Property(e => e.Timezone).HasColumnName("timezone").HasMaxLength(60).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("TEXT").IsRequired();
        });

        modelBuilder.Entity<ActivityEvent>(entity =>
        {
            entity.ToTable("activity_events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(80).IsRequired();
            entity.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(40).IsRequired();
            entity.Property(e => e.OccurredAt).HasColumnName("occurred_at").HasColumnType("TEXT").IsRequired();
            entity.Property(e => e.DurationSeconds).HasColumnName("duration_seconds");
            entity.Property(e => e.Outcome).HasColumnName("outcome").HasMaxLength(40);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .HasPrincipalKey(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ActivityEvent
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public string Location { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Outcome { get; set; }
}
