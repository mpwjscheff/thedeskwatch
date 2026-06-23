namespace TheDeskWatch.MobileApp.Contracts.Repositories;

public sealed record ColleagueRecord(string Name, string HexColor);

public interface IColleagueRepository
{
    Task<IReadOnlyList<ColleagueRecord>> GetAllAsync(CancellationToken cancellationToken = default);
}
