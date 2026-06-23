namespace TheDeskWatch.Domain;

public sealed class DeskDeparture
{
    public int Id { get; set; }
    public int ColleagueId { get; set; }
    public DateTimeOffset DepartedAt { get; set; }
    public Colleague Colleague { get; set; } = null!;
}
