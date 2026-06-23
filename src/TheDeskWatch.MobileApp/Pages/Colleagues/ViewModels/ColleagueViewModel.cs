namespace TheDeskWatch.MobileApp.Pages.Colleagues.ViewModels;

public sealed class ColleagueViewModel(string name, Color bubbleColor)
{
    public string Name { get; } = name;

    public Color BubbleColor { get; } = bubbleColor;
}
