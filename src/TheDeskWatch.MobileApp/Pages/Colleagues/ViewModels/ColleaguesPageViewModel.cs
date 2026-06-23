using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using TheDeskWatch.Application.Features.Colleagues.Queries;

namespace TheDeskWatch.MobileApp.Pages.Colleagues.ViewModels;

public sealed partial class ColleaguesPageViewModel : ObservableObject
{
    private readonly IQueryMediator _queryMediator;
    private readonly ICommandMediator _commandMediator;

    [ObservableProperty]
    private ObservableCollection<ColleagueListItemViewModel> _colleagues = [];

    public ColleaguesPageViewModel(IQueryMediator queryMediator, ICommandMediator commandMediator)
    {
        _queryMediator = queryMediator;
        _commandMediator = commandMediator;
        Title = "Colleagues";
    }

    public string Title { get; }

    public async Task LoadAsync()
    {
        var result = await _queryMediator.QueryAsync(new GetColleaguesQuery());
        result.Switch(
            response =>
            {
                Colleagues = new ObservableCollection<ColleagueListItemViewModel>(
                    response.Colleagues.Select(dto => new ColleagueListItemViewModel(dto.Id, dto.Name, dto.HexColor)));
            },
            _ => { });
    }
}
