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
        _ = ((ColleaguesPageViewModel)BindingContext).LoadAsync();
    }
}
