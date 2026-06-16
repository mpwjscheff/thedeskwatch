using Microsoft.EntityFrameworkCore;

namespace TheDeskWatch.Persistence.Data;

public sealed class DeskWatchDbContext(DbContextOptions<DeskWatchDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeskWatchDbContext).Assembly);
    }
}
