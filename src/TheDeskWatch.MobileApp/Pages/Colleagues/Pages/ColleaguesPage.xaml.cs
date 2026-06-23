using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using TheDeskWatch.MobileApp.Pages.Colleagues.ViewModels;
using TheDeskWatch.MobileApp.Pages.Colleagues.Views;

namespace TheDeskWatch.MobileApp.Pages.Colleagues.Pages;

public partial class ColleaguesPage : ContentPage
{
    private readonly ColleaguesPageViewModel _viewModel;
    private readonly ColleagueFormPopup _formPopup;

    public ColleaguesPage(ColleaguesPageViewModel viewModel, ColleagueFormPopup formPopup)
    {
        _viewModel = viewModel;
        _formPopup = formPopup;
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
        _formPopup.PrepareForAdd();
        await this.ShowPopupAsync(_formPopup, PopupOptions.Empty);
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

        _formPopup.PrepareForEdit(item.Id, item.Name, item.HexColor);
        await this.ShowPopupAsync(_formPopup, PopupOptions.Empty);
        await _viewModel.LoadAsync();
    }
}
