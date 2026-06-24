using TheDeskWatch.Application.Features.Stats.Queries;
using TheDeskWatch.MobileApp.Contracts.Repositories;

namespace TheDeskWatch.Application.Tests.Features.Stats.Queries;

public class GetStatsQueryTests
{
    private static DeskDepartureRecord Departure(int id, int hour, int minute) =>
        new(id, id, new DateTimeOffset(2026, 6, 24, hour, minute, 0, TimeSpan.Zero));

    [Fact]
    public async Task HandleAsync_ReturnsApiError_WhenRepositoryThrows()
    {
        var handler = new GetStatsQueryHandler(new ThrowingDeskDepartureRepository());

        var result = await handler.HandleAsync(new GetStatsQuery());

        Assert.True(result.IsT1);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyStats_WhenNoDepartures()
    {
        var handler = new GetStatsQueryHandler(new StubDeskDepartureRepository([]));

        var result = await handler.HandleAsync(new GetStatsQuery());

        Assert.True(result.IsT0);
        Assert.Empty(result.AsT0.Exodus.SlotData);
        Assert.Equal(default, result.AsT0.Exodus.PeakTime);
        Assert.Empty(result.AsT0.Coffee.HourData);
        Assert.Equal(0, result.AsT0.Coffee.CaffeineEventThreshold);
        Assert.Equal(0, result.AsT0.Lunch.PostLunchCount);
        Assert.False(result.AsT0.Lunch.IsFoodComaMigration);
    }

    [Fact]
    public async Task HandleAsync_GreatExodus_PicksTheBusiestFifteenMinuteSlot()
    {
        // 17:00 and 17:10 fall in the same 15-min bucket (3 departures); 09:05 is its own bucket.
        var handler = new GetStatsQueryHandler(new StubDeskDepartureRepository(
        [
            Departure(1, 9, 5),
            Departure(2, 17, 0),
            Departure(3, 17, 10),
            Departure(4, 17, 5),
        ]));

        var result = await handler.HandleAsync(new GetStatsQuery());

        Assert.True(result.IsT0);
        Assert.Equal(new TimeOnly(17, 0), result.AsT0.Exodus.PeakTime);
        Assert.Equal(2, result.AsT0.Exodus.SlotData.Count);
        Assert.Equal("09:00", result.AsT0.Exodus.SlotData[0].SlotLabel);
        Assert.Equal("17:00", result.AsT0.Exodus.SlotData[1].SlotLabel);
        Assert.Equal(3, result.AsT0.Exodus.SlotData[1].Count);
    }

    [Fact]
    public async Task HandleAsync_CoffeeOClock_GroupsByHourAndReportsThreshold()
    {
        var handler = new GetStatsQueryHandler(new StubDeskDepartureRepository(
        [
            Departure(1, 9, 0),
            Departure(2, 9, 30),
            Departure(3, 15, 0),
        ]));

        var result = await handler.HandleAsync(new GetStatsQuery());

        Assert.True(result.IsT0);
        Assert.Equal(2, result.AsT0.Coffee.HourData.Count);
        Assert.Equal("09:00", result.AsT0.Coffee.HourData[0].HourLabel);
        Assert.Equal(2, result.AsT0.Coffee.HourData[0].Count);
        Assert.Equal("15:00", result.AsT0.Coffee.HourData[1].HourLabel);
        Assert.Equal(2, result.AsT0.Coffee.CaffeineEventThreshold);
    }

    [Fact]
    public async Task HandleAsync_PostLunchDrift_FlagsFoodComaMigration()
    {
        // 3 lunch departures (12-13) -> per-hour 1.5; one other hour with 1 -> avg 1.0.
        var handler = new GetStatsQueryHandler(new StubDeskDepartureRepository(
        [
            Departure(1, 12, 0),
            Departure(2, 12, 30),
            Departure(3, 13, 0),
            Departure(4, 17, 0),
        ]));

        var result = await handler.HandleAsync(new GetStatsQuery());

        Assert.True(result.IsT0);
        Assert.Equal(3, result.AsT0.Lunch.PostLunchCount);
        Assert.Equal(1.0, result.AsT0.Lunch.RestOfDayAvgPerHour);
        Assert.True(result.AsT0.Lunch.IsFoodComaMigration);
    }

    [Fact]
    public async Task HandleAsync_PostLunchDrift_DoesNotFlag_WhenLunchIsQuiet()
    {
        var handler = new GetStatsQueryHandler(new StubDeskDepartureRepository(
        [
            Departure(1, 12, 0),
            Departure(2, 9, 0),
            Departure(3, 15, 0),
            Departure(4, 17, 0),
        ]));

        var result = await handler.HandleAsync(new GetStatsQuery());

        Assert.True(result.IsT0);
        Assert.Equal(1, result.AsT0.Lunch.PostLunchCount);
        Assert.Equal(1.0, result.AsT0.Lunch.RestOfDayAvgPerHour);
        Assert.False(result.AsT0.Lunch.IsFoodComaMigration);
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
