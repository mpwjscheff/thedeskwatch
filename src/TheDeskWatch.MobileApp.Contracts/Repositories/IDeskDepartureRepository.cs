namespace TheDeskWatch.MobileApp.Contracts.Repositories;

public sealed record DeskDepartureRecord(int Id, int ColleagueId, DateTimeOffset DepartedAt);

public interface IDeskDepartureRepository
{
    Task<IReadOnlyList<DeskDepartureRecord>> GetAllAsync(CancellationToken cancellationToken = default);
}
