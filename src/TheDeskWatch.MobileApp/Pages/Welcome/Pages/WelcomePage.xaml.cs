using TheDeskWatch.MobileApp.Pages.Welcome.ViewModels;

namespace TheDeskWatch.MobileApp.Pages.Welcome.Pages;

public partial class WelcomePage : ContentPage
{
    public WelcomePage(WelcomePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
