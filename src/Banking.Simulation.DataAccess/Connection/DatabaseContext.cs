using Banking.Simulation.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Banking.Simulation.DataAccess.Connection;

public class DatabaseContext : DbContext
{
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<WebhookConfig> WebhookConfigs { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
    }
}
