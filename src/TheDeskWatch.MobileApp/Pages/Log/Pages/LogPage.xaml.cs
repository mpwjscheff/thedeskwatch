using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.Messaging;
using TheDeskWatch.MobileApp.Pages.Log.Messages;
using TheDeskWatch.MobileApp.Pages.Log.ViewModels;

namespace TheDeskWatch.MobileApp.Pages.Log.Pages;

public partial class LogPage : ContentPage
{
    public LogPage(LogPageViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        WeakReferenceMessenger.Default.Register<StandUpToastMessage>(this, async (_, msg) =>
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Toast.Make($"{msg.FirstName} +1").Show()));
        _ = ((LogPageViewModel)BindingContext).LoadAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<StandUpToastMessage>(this);
    }
}
