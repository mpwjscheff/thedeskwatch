namespace TheDeskWatch.MobileApp.Pages.Colleagues.ViewModels;

public sealed class ColleagueListItemViewModel(int id, string name, string hexColor)
{
    public int Id { get; } = id;
    public string Name { get; } = name;
    public string HexColor { get; } = hexColor;
}
