using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteBus.Commands.Abstractions;
using TheDeskWatch.Application.Features.Colleagues.Commands;

namespace TheDeskWatch.MobileApp.Pages.Colleagues.ViewModels;

public sealed partial class ColleagueDetailPageViewModel : ObservableObject
{
    private const string DefaultHexColor = "#E53935";

    private readonly ICommandMediator _commandMediator;

    private bool _isEditMode;
    private int? _editId;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private Color _pickedColor = Color.FromArgb(DefaultHexColor);

    public ColleagueDetailPageViewModel(ICommandMediator commandMediator)
    {
        _commandMediator = commandMediator;
        Title = "Colleague";
    }

    public string Title { get; }

    public event EventHandler? SaveCompleted;

    public void Initialize(int? id, string? name, string? hexColor)
    {
        _isEditMode = id.HasValue;
        _editId = id;
        Name = name ?? string.Empty;
        PickedColor = hexColor is null
            ? Color.FromArgb(DefaultHexColor)
            : Color.FromArgb(hexColor);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        var hexColor =
            $"#{(int)(PickedColor.Red * 255):X2}{(int)(PickedColor.Green * 255):X2}{(int)(PickedColor.Blue * 255):X2}";

        if (_isEditMode)
        {
            var result = await _commandMediator.SendAsync(
                new UpdateColleagueCommand(_editId!.Value, Name.Trim(), hexColor));
            result.Switch(_ => SaveCompleted?.Invoke(this, EventArgs.Empty), _ => { });
        }
        else
        {
            var result = await _commandMediator.SendAsync(
                new AddColleagueCommand(Name.Trim(), hexColor));
            result.Switch(_ => SaveCompleted?.Invoke(this, EventArgs.Empty), _ => { });
        }
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Name);

    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
}
