using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteBus.Queries.Abstractions;
using TheDeskWatch.Application.Features.Colleagues.Queries;
using TheDeskWatch.MobileApp.Converters;

namespace TheDeskWatch.MobileApp.Pages.Colleagues.ViewModels;

public sealed partial class ColleaguesPageViewModel : ObservableObject
{
    private readonly IQueryMediator _queryMediator;
    private readonly HexStringToColorConverter _colorConverter = new();
    private readonly Dictionary<string, int> _standUpCounts = [];

    [ObservableProperty]
    private ObservableCollection<ColleagueViewModel> _colleagues = [];

    public ColleaguesPageViewModel(IQueryMediator queryMediator)
    {
        _queryMediator = queryMediator;
    }

    public string Title { get; } = "Colleagues";

    public async Task LoadAsync()
    {
        var result = await _queryMediator.QueryAsync(new GetColleaguesQuery());

        result.Switch(
            response =>
            {
                Colleagues = new ObservableCollection<ColleagueViewModel>(
                    response.Colleagues.Select(dto => new ColleagueViewModel(
                        dto.Name,
                        (Color)(_colorConverter.Convert(dto.HexColor, typeof(Color), null, null) ?? Colors.Gray))));
            },
            _ => { });
    }

    [RelayCommand]
    private void RegisterStandUp(ColleagueViewModel colleague)
    {
        _standUpCounts.TryGetValue(colleague.Name, out var current);
        _standUpCounts[colleague.Name] = current + 1;
    }
}
