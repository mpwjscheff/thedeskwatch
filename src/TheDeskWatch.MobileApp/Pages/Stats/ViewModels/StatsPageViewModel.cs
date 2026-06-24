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

    [ObservableProperty]
    private ObservableCollection<ChartDataPoint> _weeklyDepartureChartData = [];

    [ObservableProperty]
    private string _peakEscapeTimeLabel = string.Empty;

    [ObservableProperty]
    private bool _isFoodComaMigration;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public StatsPageViewModel(IQueryMediator queryMediator)
    {
        _queryMediator = queryMediator;
        Title = "Stats";
        IsFoodComaMigration = false;
    }

    public string Title { get; }

    [RelayCommand]
    private async Task LoadStats()
    {
        var statsTask = _queryMediator.QueryAsync(new GetStatsQuery());
        var weeklyTask = _queryMediator.QueryAsync(new GetWeeklyDepartureStatsQuery());

        await Task.WhenAll(statsTask, weeklyTask);

        var result = await statsTask;
        var weeklyResult = await weeklyTask;

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
            error => ErrorMessage = $"Failed to load stats: {error.Message}");

        weeklyResult.Switch(
            response =>
            {
                WeeklyDepartureChartData = new ObservableCollection<ChartDataPoint>(
                    response.Days.Select(dto =>
                        new ChartDataPoint(dto.DayLabel, dto.Count)));
            },
            error =>
            {
                var weeklyError = $"Failed to load weekly departures: {error.Message}";
                ErrorMessage = string.IsNullOrEmpty(ErrorMessage)
                    ? weeklyError
                    : $"{ErrorMessage}{Environment.NewLine}{weeklyError}";
            });
    }
}
