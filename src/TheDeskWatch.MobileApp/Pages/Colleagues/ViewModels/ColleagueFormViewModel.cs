using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteBus.Commands.Abstractions;
using TheDeskWatch.Application.Features.Colleagues.Commands;

namespace TheDeskWatch.MobileApp.Pages.Colleagues.ViewModels;

public sealed partial class ColleagueFormViewModel : ObservableObject
{
    private readonly ICommandMediator _commandMediator;

    private bool _isEditMode;
    private int? _editId;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private ColleagueColorSwatch? _selectedSwatch;

    public ColleagueFormViewModel(ICommandMediator commandMediator)
    {
        _commandMediator = commandMediator;
        AvailableSwatches =
        [
            new ColleagueColorSwatch("#E53935"),
            new ColleagueColorSwatch("#D81B60"),
            new ColleagueColorSwatch("#8E24AA"),
            new ColleagueColorSwatch("#3949AB"),
            new ColleagueColorSwatch("#1E88E5"),
            new ColleagueColorSwatch("#00897B"),
            new ColleagueColorSwatch("#43A047"),
            new ColleagueColorSwatch("#C0CA33"),
            new ColleagueColorSwatch("#FB8C00"),
            new ColleagueColorSwatch("#6D4C41"),
        ];
    }

    public IReadOnlyList<ColleagueColorSwatch> AvailableSwatches { get; }

    public event EventHandler? SaveCompleted;

    public void Initialize(int? id, string? name, string? hexColor)
    {
        _isEditMode = id.HasValue;
        _editId = id;
        Name = name ?? string.Empty;
        SelectedSwatch = hexColor is null
            ? null
            : AvailableSwatches.FirstOrDefault(s => s.HexColor == hexColor);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        if (_isEditMode)
        {
            var result = await _commandMediator.SendAsync(
                new UpdateColleagueCommand(_editId!.Value, Name.Trim(), SelectedSwatch!.HexColor));
            result.Switch(_ => SaveCompleted?.Invoke(this, EventArgs.Empty), _ => { });
        }
        else
        {
            var result = await _commandMediator.SendAsync(
                new AddColleagueCommand(Name.Trim(), SelectedSwatch!.HexColor));
            result.Switch(_ => SaveCompleted?.Invoke(this, EventArgs.Empty), _ => { });
        }
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Name) && SelectedSwatch is not null;

    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

    partial void OnSelectedSwatchChanged(ColleagueColorSwatch? value) => SaveCommand.NotifyCanExecuteChanged();
}
