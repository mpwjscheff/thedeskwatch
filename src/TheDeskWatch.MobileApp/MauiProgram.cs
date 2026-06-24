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
using TheDeskWatch.MobileApp.Pages.Stats.Pages;
using TheDeskWatch.MobileApp.Pages.Stats.ViewModels;
using TheDeskWatch.Persistence;
using Syncfusion.Maui.Toolkit.Hosting;

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
            .ConfigureSyncfusionToolkit()
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

        // Stats page
        builder.Services.AddTransient<StatsPage>();
        builder.Services.AddTransient<StatsPageViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        app.Services.InitializeDatabase();
        return app;
    }
}
