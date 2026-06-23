namespace TheDeskWatch.MobileApp.Pages.Log.ViewModels;

public sealed class ColleagueViewModel(string name, Color bubbleColor)
{
    public string Name { get; } = name;
    public Color BubbleColor { get; } = bubbleColor;
}
