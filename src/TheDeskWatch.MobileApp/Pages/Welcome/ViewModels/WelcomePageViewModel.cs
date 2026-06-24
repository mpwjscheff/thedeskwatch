using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApplication = Microsoft.Maui.Controls.Application;

namespace TheDeskWatch.MobileApp.Pages.Welcome.ViewModels;

public sealed partial class WelcomePageViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;

    public WelcomePageViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "Welcome";
    }

    public string Title { get; }

    [RelayCommand]
    private void GetStarted()
    {
        var shell = _serviceProvider.GetRequiredService<AppShell>();
        if (MauiApplication.Current is { } app && app.Windows.Count > 0)
            app.Windows[0].Page = shell;
    }
}
