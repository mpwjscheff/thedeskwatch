using TheDeskWatch.MobileApp.Pages.Colleagues.ViewModels;

namespace TheDeskWatch.MobileApp.Pages.Colleagues.Pages;

public partial class ColleaguesPage : ContentPage
{
    private readonly ColleaguesPageViewModel _viewModel;
    private readonly ColleagueDetailPage _detailPage;

    public ColleaguesPage(ColleaguesPageViewModel viewModel, ColleagueDetailPage detailPage)
    {
        _viewModel = viewModel;
        _detailPage = detailPage;
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.LoadAsync();
    }

    private async void OnAddTapped(object? sender, EventArgs e)
    {
        _detailPage.PrepareForAdd();
        await Navigation.PushModalAsync(new NavigationPage(_detailPage));
        await _viewModel.LoadAsync();
    }

    private async void OnColleagueSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0 ||
            e.CurrentSelection[0] is not ColleagueListItemViewModel item)
        {
            return;
        }

        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }

        _detailPage.PrepareForEdit(item.Id, item.Name, item.HexColor);
        await Navigation.PushModalAsync(new NavigationPage(_detailPage));
        await _viewModel.LoadAsync();
    }
}
