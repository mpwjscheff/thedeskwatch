using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteBus.Queries.Abstractions;
using TheDeskWatch.Application.Features.Stats.Queries;

namespace TheDeskWatch.MobileApp.Pages.Stats.ViewModels;

public sealed record ChartDataPoint(string Label, double Value, string DataLabel = "");

public sealed partial class StatsPageViewModel : ObservableObject
{
    private readonly IQueryMediator _queryMediator;

    [ObservableProperty]
    private ObservableCollection<ChartDataPoint> _exodusChartData = [];

    [ObservableProperty]
    private ObservableCollection<ChartDataPoint> _coffeeChartData = [];

    [ObservableProperty]
    private ObservableCollection<ChartDataPoint> _lunchChartData = [];

    public StatsPageViewModel(IQueryMediator queryMediator)
    {
        _queryMediator = queryMediator;
        Title = "Stats";
        IsFoodComaMigration = false;
        LoadStatsCommand.Execute(null);
    }

    public string Title { get; }

    public string PeakEscapeTimeLabel { get; private set; } = string.Empty;

    public bool IsFoodComaMigration { get; private set; }

    [RelayCommand]
    private async Task LoadStats()
    {
        var result = await _queryMediator.QueryAsync(new GetStatsQuery());
        result.Switch(
            response =>
            {
                ExodusChartData = new ObservableCollection<ChartDataPoint>(
                    response.Exodus.SlotData.Select(slot =>
                        new ChartDataPoint(slot.SlotLabel, slot.Count)));

                PeakEscapeTimeLabel =
                    $"Peak escape time: {response.Exodus.PeakTime.ToString("hh:mm tt", CultureInfo.CurrentCulture)}";

                CoffeeChartData = new ObservableCollection<ChartDataPoint>(
                    response.Coffee.HourData.Select(hour =>
                        new ChartDataPoint(
                            hour.HourLabel,
                            hour.Count,
                            hour.Count >= response.Coffee.CaffeineEventThreshold
                                ? "☕ caffeine event"
                                : string.Empty)));

                LunchChartData =
                [
                    new ChartDataPoint("Post-lunch (2h)", response.Lunch.PostLunchCount),
                    new ChartDataPoint(
                        "Rest of day (avg/2h)",
                        Math.Round(response.Lunch.RestOfDayAvgPerHour * 2, 1)),
                ];

                IsFoodComaMigration = response.Lunch.IsFoodComaMigration;
            },
            _ => { });
    }
}
