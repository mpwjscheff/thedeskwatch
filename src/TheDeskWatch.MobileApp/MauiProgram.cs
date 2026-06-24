using CommunityToolkit.Maui;
using LiteBus.Commands;
using LiteBus.Extensions.Microsoft.DependencyInjection;
using LiteBus.Queries;
using Microsoft.Extensions.Logging;
using TheDeskWatch.Application.Features.Colleagues.Queries;
using SkiaSharp.Views.Maui.Controls.Hosting;
using TheDeskWatch.MobileApp.Pages.Colleagues.Pages;
using TheDeskWatch.MobileApp.Pages.Colleagues.ViewModels;
using TheDeskWatch.MobileApp.Pages.Log.Pages;
using TheDeskWatch.MobileApp.Pages.Log.ViewModels;
using TheDeskWatch.MobileApp.Pages.Welcome.Pages;
using TheDeskWatch.MobileApp.Pages.Welcome.ViewModels;
using TheDeskWatch.Persistence;

namespace TheDeskWatch.MobileApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSkiaSharp()
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

        // Log page (renamed from old Colleagues)
        builder.Services.AddTransient<LogPage>();
        builder.Services.AddTransient<LogPageViewModel>();

        // Colleagues list page
        builder.Services.AddTransient<ColleaguesPage>();
        builder.Services.AddTransient<ColleaguesPageViewModel>();

        // Colleague detail page (shown as a swipe-to-dismiss modal)
        builder.Services.AddTransient<ColleagueDetailPage>();
        builder.Services.AddTransient<ColleagueDetailPageViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddTransient<AppShell>();
        builder.Services.AddTransient<WelcomePage>();
        builder.Services.AddTransient<WelcomePageViewModel>();

        var app = builder.Build();
        app.Services.InitializeDatabase();
        return app;
    }
}
