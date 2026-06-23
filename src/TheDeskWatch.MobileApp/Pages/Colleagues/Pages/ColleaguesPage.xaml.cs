using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.Messaging;
using TheDeskWatch.MobileApp.Pages.Colleagues.Messages;
using TheDeskWatch.MobileApp.Pages.Colleagues.ViewModels;

namespace TheDeskWatch.MobileApp.Pages.Colleagues.Pages;

public partial class ColleaguesPage : ContentPage
{
    public ColleaguesPage(ColleaguesPageViewModel viewModel)
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

        _ = ((ColleaguesPageViewModel)BindingContext).LoadAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<StandUpToastMessage>(this);
    }
}
