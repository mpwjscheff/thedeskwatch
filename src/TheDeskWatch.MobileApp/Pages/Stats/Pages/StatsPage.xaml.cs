using TheDeskWatch.MobileApp.Pages.Stats.ViewModels;

namespace TheDeskWatch.MobileApp.Pages.Stats.Pages;

public partial class StatsPage : ContentPage
{
    private readonly StatsPageViewModel _viewModel;

    public StatsPage(StatsPageViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.LoadStatsCommand.ExecuteAsync(null);
    }
}
