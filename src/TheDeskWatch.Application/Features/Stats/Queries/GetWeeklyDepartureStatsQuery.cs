using LiteBus.Queries.Abstractions;
using OneOf;
using System.Globalization;
using TheDeskWatch.Application.Common;
using TheDeskWatch.MobileApp.Contracts.Repositories;

namespace TheDeskWatch.Application.Features.Stats.Queries;

public record GetWeeklyDepartureStatsQuery : IQuery<OneOf<GetWeeklyDepartureStatsResponse, ApiError>>;

public sealed record GetWeeklyDepartureStatsResponse(
    IReadOnlyList<GetWeeklyDepartureStatsResponse.WeeklyDayDataPointDto> Days)
{
    public sealed record WeeklyDayDataPointDto(string DayLabel, int Count);
}

public sealed class GetWeeklyDepartureStatsQueryHandler(IDeskDepartureRepository repository)
    : IQueryHandler<GetWeeklyDepartureStatsQuery, OneOf<GetWeeklyDepartureStatsResponse, ApiError>>
{
    public async Task<OneOf<GetWeeklyDepartureStatsResponse, ApiError>> HandleAsync(
        GetWeeklyDepartureStatsQuery query,
        CancellationToken cancellationToken = new())
    {
        try
        {
            var departures = await repository.GetAllAsync(cancellationToken);
            var days = BuildWeeklyDays(departures);

            return new GetWeeklyDepartureStatsResponse(days);
        }
        catch (Exception exception)
        {
            return new ApiError(exception.Message);
        }
    }

    private static List<GetWeeklyDepartureStatsResponse.WeeklyDayDataPointDto> BuildWeeklyDays(
        IReadOnlyList<DeskDepartureRecord> departures)
    {
        var (previousMonday, previousFriday) = GetPreviousWeekRange();

        var countsByDate = departures
            .Where(departure => departure.DepartedAt.Date >= previousMonday && departure.DepartedAt.Date <= previousFriday)
            .GroupBy(departure => departure.DepartedAt.Date)
            .ToDictionary(group => group.Key, group => group.Count());

        return Enumerable.Range(0, 5)
            .Select(offset =>
            {
                var date = previousMonday.AddDays(offset);
                var label = date.ToString("ddd", CultureInfo.InvariantCulture);
                var count = countsByDate.TryGetValue(date, out var value) ? value : 0;
                return new GetWeeklyDepartureStatsResponse.WeeklyDayDataPointDto(label, count);
            })
            .ToList();
    }

    private static (DateTime PreviousMonday, DateTime PreviousFriday) GetPreviousWeekRange()
    {
        var previousMonday = new DateTime(2026, 5, 25);
        var previousFriday = new DateTime(2026, 5, 29);

        return (previousMonday, previousFriday);
    }
}
