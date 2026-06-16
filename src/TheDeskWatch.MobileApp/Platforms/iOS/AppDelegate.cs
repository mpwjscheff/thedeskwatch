using Foundation;

namespace TheDeskWatch.MobileApp;

#pragma warning disable CA1711 // iOS platform requires this exact class name
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
#pragma warning restore CA1711
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}