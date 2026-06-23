using System.Diagnostics;

namespace TheDeskWatch.MobileApp;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();
        WireGlobalExceptionHandlers();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    private static void WireGlobalExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            Debug.WriteLine($"[UnhandledException] {args.ExceptionObject as Exception}");
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            Debug.WriteLine($"[UnobservedTaskException] {args.Exception}");
            args.SetObserved();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var windows = Current?.Windows;
                var page = windows is { Count: > 0 } ? windows[0].Page : null;
                if (page is not null)
                    await page.DisplayAlertAsync(
                        "Unexpected Error",
                        "Something went wrong. Please restart the app if the issue persists.",
                        "OK");
            });
        };
    }
}