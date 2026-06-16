using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheDeskWatch.Persistence.Data;

namespace TheDeskWatch.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string databasePath)
    {
        services.AddDbContext<DeskWatchDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        return services;
    }

    public static void InitializeDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<DeskWatchDbContext>()
            .Database.EnsureCreated();
    }
}
