using TheDeskWatch.Application.Features.Stats.Queries;
using TheDeskWatch.MobileApp.Contracts.Repositories;

namespace TheDeskWatch.Application.Tests.Features.Stats.Queries;

public class GetWeeklyDepartureStatsQueryHandlerTests
{
    private static readonly string[] ExpectedWeekdayLabels = ["Mon", "Tue", "Wed", "Thu", "Fri"];

    private static DeskDepartureRecord Departure(int id, DateTimeOffset timestamp) =>
        new(id, id, timestamp);

    private static DateTimeOffset At(DateTime date, int dayOffset, int hour) =>
        new(DateTime.SpecifyKind(date.AddDays(dayOffset).AddHours(hour), DateTimeKind.Unspecified), TimeSpan.Zero);

    private static DateTime PreviousMonday()
    {
        var today = DateTime.Today;
        var thisMonday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        return thisMonday.AddDays(-7);
    }

    [Fact]
    public async Task HandleAsync_ReturnsApiError_WhenRepositoryThrows()
    {
        var handler = new GetWeeklyDepartureStatsQueryHandler(new ThrowingDeskDepartureRepository());

        var result = await handler.HandleAsync(new GetWeeklyDepartureStatsQuery());

        Assert.True(result.IsT1);
    }

    [Fact]
    public async Task HandleAsync_ReturnsFiveOrderedWeekdays_WhenEachWeekdayHasADeparture()
    {
        var monday = PreviousMonday();
        var handler = new GetWeeklyDepartureStatsQueryHandler(new StubDeskDepartureRepository(
        [
            Departure(1, At(monday, 0, 9)),
            Departure(2, At(monday, 1, 10)),
            Departure(3, At(monday, 2, 11)),
            Departure(4, At(monday, 3, 12)),
            Departure(5, At(monday, 4, 13)),
        ]));

        var result = await handler.HandleAsync(new GetWeeklyDepartureStatsQuery());

        Assert.True(result.IsT0);
        Assert.Equal(5, result.AsT0.Days.Count);
        Assert.Equal(
            ExpectedWeekdayLabels,
            result.AsT0.Days.Select(day => day.DayLabel));
        Assert.All(result.AsT0.Days, day => Assert.Equal(1, day.Count));
    }

    [Fact]
    public async Task HandleAsync_ZeroFillsDays_WhenSomeWeekdaysHaveNoDepartures()
    {
        var monday = PreviousMonday();
        var handler = new GetWeeklyDepartureStatsQueryHandler(new StubDeskDepartureRepository(
        [
            Departure(1, At(monday, 0, 9)),
            Departure(2, At(monday, 2, 9)),
        ]));

        var result = await handler.HandleAsync(new GetWeeklyDepartureStatsQuery());

        Assert.True(result.IsT0);
        Assert.Equal(5, result.AsT0.Days.Count);
        Assert.Equal(1, result.AsT0.Days[0].Count);
        Assert.Equal(0, result.AsT0.Days[1].Count);
        Assert.Equal(1, result.AsT0.Days[2].Count);
        Assert.Equal(0, result.AsT0.Days[3].Count);
        Assert.Equal(0, result.AsT0.Days[4].Count);
    }

    [Fact]
    public async Task HandleAsync_ExcludesDepartures_FromTheCurrentWeek()
    {
        var currentWeekMonday = PreviousMonday().AddDays(7);
        var handler = new GetWeeklyDepartureStatsQueryHandler(new StubDeskDepartureRepository(
        [
            Departure(1, At(currentWeekMonday, 0, 9)),
        ]));

        var result = await handler.HandleAsync(new GetWeeklyDepartureStatsQuery());

        Assert.True(result.IsT0);
        Assert.All(result.AsT0.Days, day => Assert.Equal(0, day.Count));
    }

    [Fact]
    public async Task HandleAsync_ExcludesDepartures_FromTwoWeeksAgo()
    {
        var twoWeeksAgoMonday = PreviousMonday().AddDays(-7);
        var handler = new GetWeeklyDepartureStatsQueryHandler(new StubDeskDepartureRepository(
        [
            Departure(1, At(twoWeeksAgoMonday, 0, 9)),
        ]));

        var result = await handler.HandleAsync(new GetWeeklyDepartureStatsQuery());

        Assert.True(result.IsT0);
        Assert.All(result.AsT0.Days, day => Assert.Equal(0, day.Count));
    }

    private sealed class StubDeskDepartureRepository(IReadOnlyList<DeskDepartureRecord> departures) : IDeskDepartureRepository
    {
        public Task<IReadOnlyList<DeskDepartureRecord>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(departures);
    }

    private sealed class ThrowingDeskDepartureRepository : IDeskDepartureRepository
    {
        public Task<IReadOnlyList<DeskDepartureRecord>> GetAllAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Repository failure.");
    }
}
