using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheDeskWatch.MobileApp.Contracts.Repositories;
using TheDeskWatch.Persistence.Data;
using TheDeskWatch.Persistence.Repositories;

namespace TheDeskWatch.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string databasePath)
    {
        services.AddDbContext<DeskWatchDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        services.AddScoped<IColleagueRepository, ColleagueRepository>();

        return services;
    }

    public static void InitializeDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeskWatchDbContext>();
        context.Database.EnsureCreated();
        DatabaseSeeder.Seed(context);
    }
}
