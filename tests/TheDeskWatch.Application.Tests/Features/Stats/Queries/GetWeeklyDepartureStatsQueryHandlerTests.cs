using TheDeskWatch.Application.Features.Stats.Queries;
using TheDeskWatch.MobileApp.Contracts.Repositories;

namespace TheDeskWatch.Application.Tests.Features.Stats.Queries;

public class GetWeeklyDepartureStatsQueryHandlerTests
{
    private static readonly string[] ExpectedWeekdayLabels = ["Mon", "Tue", "Wed", "Thu", "Fri"];

    private static DeskDepartureRecord Departure(int id, int year, int month, int day, int hour = 9) =>
        new(id, id, new DateTimeOffset(DateTime.SpecifyKind(new DateTime(year, month, day, hour, 0, 0), DateTimeKind.Unspecified), TimeSpan.Zero));

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
        var handler = new GetWeeklyDepartureStatsQueryHandler(new StubDeskDepartureRepository(
        [
            Departure(1, 2026, 5, 25, 9),
            Departure(2, 2026, 5, 26, 10),
            Departure(3, 2026, 5, 27, 11),
            Departure(4, 2026, 5, 28, 12),
            Departure(5, 2026, 5, 29, 13),
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
        var handler = new GetWeeklyDepartureStatsQueryHandler(new StubDeskDepartureRepository(
        [
            Departure(1, 2026, 5, 25, 9),
            Departure(2, 2026, 5, 27, 9),
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
        var handler = new GetWeeklyDepartureStatsQueryHandler(new StubDeskDepartureRepository(
        [
            Departure(1, 2026, 6, 1, 9),
        ]));

        var result = await handler.HandleAsync(new GetWeeklyDepartureStatsQuery());

        Assert.True(result.IsT0);
        Assert.All(result.AsT0.Days, day => Assert.Equal(0, day.Count));
    }

    [Fact]
    public async Task HandleAsync_ExcludesDepartures_FromTwoWeeksAgo()
    {
        var handler = new GetWeeklyDepartureStatsQueryHandler(new StubDeskDepartureRepository(
        [
            Departure(1, 2026, 5, 18, 9),
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
