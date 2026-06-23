namespace TheDeskWatch.MobileApp.Pages.Colleagues.ViewModels;

public sealed record ColleagueColorSwatch(string HexColor)
{
    public Color Color => Color.FromArgb(HexColor);
}
