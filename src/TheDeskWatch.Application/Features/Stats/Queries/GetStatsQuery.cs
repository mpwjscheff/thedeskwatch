using LiteBus.Queries.Abstractions;
using OneOf;
using System.Globalization;
using TheDeskWatch.Application.Common;
using TheDeskWatch.MobileApp.Contracts.Repositories;

namespace TheDeskWatch.Application.Features.Stats.Queries;

public record GetStatsQuery : IQuery<OneOf<GetStatsResponse, ApiError>>;

public sealed record GetStatsResponse(
    GetStatsResponse.GreatExodusDto Exodus,
    GetStatsResponse.CoffeeOClockDto Coffee,
    GetStatsResponse.PostLunchDriftDto Lunch)
{
    public sealed record GreatExodusDto(TimeOnly PeakTime, IReadOnlyList<TimeSlotDataPointDto> SlotData);

    public sealed record CoffeeOClockDto(IReadOnlyList<HourDataPointDto> HourData, int CaffeineEventThreshold);

    public sealed record PostLunchDriftDto(int PostLunchCount, double RestOfDayAvgPerHour, bool IsFoodComaMigration);

    public sealed record TimeSlotDataPointDto(string SlotLabel, int Count);

    public sealed record HourDataPointDto(string HourLabel, int Count);
}

public sealed class GetStatsQueryHandler(IDeskDepartureRepository repository)
    : IQueryHandler<GetStatsQuery, OneOf<GetStatsResponse, ApiError>>
{
    public async Task<OneOf<GetStatsResponse, ApiError>> HandleAsync(
        GetStatsQuery query,
        CancellationToken cancellationToken = new())
    {
        try
        {
            var departures = await repository.GetAllAsync(cancellationToken);
            var times = departures.Select(departure => departure.DepartedAt).ToList();

            return new GetStatsResponse(
                BuildGreatExodus(times),
                BuildCoffeeOClock(times),
                BuildPostLunchDrift(times));
        }
        catch (Exception exception)
        {
            return new ApiError(exception.Message);
        }
    }

    private static GetStatsResponse.GreatExodusDto BuildGreatExodus(List<DateTimeOffset> times)
    {
        var slots = times
            .GroupBy(time => ((time.Hour * 60) + time.Minute) / 15)
            .Select(group => new { BucketIndex = group.Key, Count = group.Count() })
            .OrderBy(slot => slot.BucketIndex)
            .ToList();

        if (slots.Count == 0)
        {
            return new GetStatsResponse.GreatExodusDto(default, []);
        }

        var peakBucket = slots.MaxBy(slot => slot.Count)!.BucketIndex;
        var peakTime = BucketToTime(peakBucket);

        var slotData = slots
            .Select(slot => new GetStatsResponse.TimeSlotDataPointDto(BucketToTime(slot.BucketIndex).ToString("HH:mm", CultureInfo.InvariantCulture), slot.Count))
            .ToList();

        return new GetStatsResponse.GreatExodusDto(peakTime, slotData);
    }

    private static GetStatsResponse.CoffeeOClockDto BuildCoffeeOClock(List<DateTimeOffset> times)
    {
        var hours = times
            .GroupBy(time => time.Hour)
            .Select(group => new { Hour = group.Key, Count = group.Count() })
            .OrderBy(hour => hour.Hour)
            .ToList();

        if (hours.Count == 0)
        {
            return new GetStatsResponse.CoffeeOClockDto([], 0);
        }

        var threshold = hours.Max(hour => hour.Count);

        var hourData = hours
            .Select(hour => new GetStatsResponse.HourDataPointDto($"{hour.Hour:D2}:00", hour.Count))
            .ToList();

        return new GetStatsResponse.CoffeeOClockDto(hourData, threshold);
    }

    private static GetStatsResponse.PostLunchDriftDto BuildPostLunchDrift(List<DateTimeOffset> times)
    {
        var postLunchCount = times.Count(time => time.Hour is 12 or 13);

        var distinctHoursOutsideLunch = times
            .Where(time => time.Hour is not (12 or 13))
            .Select(time => time.Hour)
            .Distinct()
            .Count();

        var restOfDayCount = times.Count - postLunchCount;
        var restOfDayAvgPerHour = restOfDayCount / (double)Math.Max(1, distinctHoursOutsideLunch);
        var isFoodComaMigration = (postLunchCount / 2.0) > restOfDayAvgPerHour;

        return new GetStatsResponse.PostLunchDriftDto(postLunchCount, restOfDayAvgPerHour, isFoodComaMigration);
    }

    private static TimeOnly BucketToTime(int bucketIndex)
    {
        var totalMinutes = bucketIndex * 15;
        return new TimeOnly(totalMinutes / 60, totalMinutes % 60);
    }
}
