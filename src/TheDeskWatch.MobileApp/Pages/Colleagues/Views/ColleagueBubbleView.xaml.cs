using System.Windows.Input;

namespace TheDeskWatch.MobileApp.Pages.Colleagues.Views;

public partial class ColleagueBubbleView : ContentView
{
    public static readonly BindableProperty ColleagueNameProperty = BindableProperty.Create(
        nameof(ColleagueName),
        typeof(string),
        typeof(ColleagueBubbleView),
        string.Empty);

    public static readonly BindableProperty BubbleColorProperty = BindableProperty.Create(
        nameof(BubbleColor),
        typeof(Color),
        typeof(ColleagueBubbleView),
        Colors.Gray);

    public static readonly BindableProperty TapCommandProperty = BindableProperty.Create(
        nameof(TapCommand),
        typeof(ICommand),
        typeof(ColleagueBubbleView),
        null);

    public static readonly BindableProperty TapCommandParameterProperty = BindableProperty.Create(
        nameof(TapCommandParameter),
        typeof(object),
        typeof(ColleagueBubbleView),
        null);

    public ColleagueBubbleView()
    {
        InitializeComponent();
    }

    public string ColleagueName
    {
        get => (string)GetValue(ColleagueNameProperty);
        set => SetValue(ColleagueNameProperty, value);
    }

    public Color BubbleColor
    {
        get => (Color)GetValue(BubbleColorProperty);
        set => SetValue(BubbleColorProperty, value);
    }

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public object? TapCommandParameter
    {
        get => GetValue(TapCommandParameterProperty);
        set => SetValue(TapCommandParameterProperty, value);
    }

    public async Task PlayTapAnimationAsync()
    {
        await this.ScaleToAsync(1.15, 80);
        await this.ScaleToAsync(1.0, 80);
    }

    private async void OnTapped(object? sender, TappedEventArgs e)
    {
        await PlayTapAnimationAsync();
        TapCommand?.Execute(TapCommandParameter);
    }
}
