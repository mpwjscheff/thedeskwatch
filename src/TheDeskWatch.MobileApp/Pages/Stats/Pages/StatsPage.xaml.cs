using TheDeskWatch.MobileApp.Pages.Stats.ViewModels;

namespace TheDeskWatch.MobileApp.Pages.Stats.Pages;

public partial class StatsPage : ContentPage
{
    public StatsPage(StatsPageViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
