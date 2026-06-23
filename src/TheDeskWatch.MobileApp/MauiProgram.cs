using LiteBus.Commands;
using LiteBus.Extensions.Microsoft.DependencyInjection;
using LiteBus.Queries;
using Microsoft.Extensions.Logging;
using TheDeskWatch.Application.Features.Colleagues.Queries;
using TheDeskWatch.MobileApp.Pages.Colleagues.Pages;
using TheDeskWatch.MobileApp.Pages.Colleagues.ViewModels;
using TheDeskWatch.Persistence;

namespace TheDeskWatch.MobileApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("fa-solid-900.ttf", "FontAwesome6Solid");
            });

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "deskwatch.db");
        builder.Services.AddPersistence(dbPath);

        var applicationAssembly = typeof(GetColleaguesQuery).Assembly;
        builder.Services.AddLiteBus(config =>
            config
                .AddCommandModule(module => module.RegisterFromAssembly(applicationAssembly))
                .AddQueryModule(module => module.RegisterFromAssembly(applicationAssembly)));

        builder.Services.AddTransient<ColleaguesPage>();
        builder.Services.AddTransient<ColleaguesPageViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        app.Services.InitializeDatabase();
        return app;
    }
}
