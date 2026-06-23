using TheDeskWatch.MobileApp.Pages.Colleagues.ViewModels;

namespace TheDeskWatch.MobileApp.Pages.Colleagues.Pages;

public partial class ColleagueDetailPage : ContentPage
{
    private readonly ColleagueDetailPageViewModel _viewModel;

    public ColleagueDetailPage(ColleagueDetailPageViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
        viewModel.SaveCompleted += async (_, _) => await Navigation.PopModalAsync();
    }

    public void PrepareForAdd()
        => _viewModel.Initialize(null, null, null);

    public void PrepareForEdit(int id, string name, string hexColor)
        => _viewModel.Initialize(id, name, hexColor);

    private async void OnCancelTapped(object? sender, EventArgs e)
        => await Navigation.PopModalAsync();
}
