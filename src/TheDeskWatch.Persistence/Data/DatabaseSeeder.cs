using TheDeskWatch.Domain;
using TheDeskWatch.Persistence.Data;

namespace TheDeskWatch.Persistence;

internal static class DatabaseSeeder
{
    private static readonly (string Name, string HexColor)[] ColleagueSeedData =
    [
        ("Emma de Vries",    "#E74C3C"),
        ("Liam van den Berg","#3498DB"),
        ("Sophie Jansen",    "#2ECC71"),
        ("Noah de Boer",     "#9B59B6"),
        ("Julia Bakker",     "#F39C12"),
        ("Finn van Dijk",    "#1ABC9C"),
        ("Mila Visser",      "#E67E22"),
        ("Lars Smit",        "#2980B9"),
        ("Saar Meijer",      "#E91E63"),
        ("Bram Peters",      "#00BCD4"),
        ("Fleur Hendriks",   "#8BC34A"),
        ("Thijs van der Laan","#FF5722"),
    ];

    // May 2026 weekday day-of-month values.
    private static readonly int[] May2026Weekdays =
        [1, 4, 5, 6, 7, 8, 11, 12, 13, 14, 15, 18, 19, 20, 21, 22, 25, 26, 27, 28, 29];

    internal static void Seed(DeskWatchDbContext context)
    {
        SeedColleagues(context);
        SeedDeskDepartures(context);
    }

    private static void SeedColleagues(DeskWatchDbContext context)
    {
        if (context.Colleagues.Any())
            return;

        var colleagues = ColleagueSeedData.Select(c => new Colleague
        {
            Name = c.Name,
            HexColor = c.HexColor,
        });

        context.Colleagues.AddRange(colleagues);
        context.SaveChanges();
    }

    private static void SeedDeskDepartures(DeskWatchDbContext context)
    {
        if (context.DeskDepartures.Any())
            return;

        // Fetch persisted colleague IDs in insertion order.
        var colleagueIds = context.Colleagues
            .OrderBy(c => c.Id)
            .Select(c => c.Id)
            .ToArray();

        if (colleagueIds.Length == 0)
            return;

        var departures = new List<DeskDeparture>();

        foreach (var (day, dayIndex) in May2026Weekdays.Select((d, i) => (d, i)))
        {
            // Between 50 and 100 events per day, deterministic per day.
            var eventCount = 50 + dayIndex * 3 % 51;

            // Office hours: 08:00–17:30 → 570 minutes of spread.
            const int officeStartHour = 8;
            const int officeMinutes = 570;

            for (var i = 0; i < eventCount; i++)
            {
                var colleagueId = colleagueIds[i % colleagueIds.Length];
                var minuteOffset = i * officeMinutes / eventCount;
                var hour = officeStartHour + minuteOffset / 60;
                var minute = minuteOffset % 60;

                departures.Add(new DeskDeparture
                {
                    ColleagueId = colleagueId,
                    DepartedAt = new DateTimeOffset(2026, 5, day, hour, minute, 0, TimeSpan.Zero),
                });
            }
        }

        context.DeskDepartures.AddRange(departures);
        context.SaveChanges();
    }
}
