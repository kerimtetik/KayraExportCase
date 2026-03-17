using Log.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Log.Infrastructure.Persistence;

public class LogDbContext : DbContext
{
    public LogDbContext(DbContextOptions<LogDbContext> options) : base(options)
    {
    }

    public DbSet<LogEntry> Logs => Set<LogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LogEntry>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ServiceName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Level).IsRequired().HasMaxLength(20);
            entity.Property(x => x.Message).IsRequired().HasMaxLength(1000);
        });
    }
}