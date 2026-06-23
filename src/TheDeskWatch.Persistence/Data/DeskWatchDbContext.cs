using Microsoft.EntityFrameworkCore;
using TheDeskWatch.Domain;

namespace TheDeskWatch.Persistence.Data;

public sealed class DeskWatchDbContext(DbContextOptions<DeskWatchDbContext> options) : DbContext(options)
{
    public DbSet<Colleague> Colleagues { get; set; }
    public DbSet<DeskDeparture> DeskDepartures { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeskWatchDbContext).Assembly);
    }
}
