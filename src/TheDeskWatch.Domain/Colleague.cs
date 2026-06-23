namespace TheDeskWatch.Domain;

public sealed class Colleague
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string HexColor { get; set; }
    public ICollection<DeskDeparture> Departures { get; set; } = [];
}
