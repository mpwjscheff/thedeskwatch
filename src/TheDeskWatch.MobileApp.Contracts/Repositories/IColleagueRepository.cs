namespace TheDeskWatch.MobileApp.Contracts.Repositories;

public sealed record ColleagueRecord(int Id, string Name, string HexColor);

public interface IColleagueRepository
{
    Task<IReadOnlyList<ColleagueRecord>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(string name, string hexColor, CancellationToken cancellationToken = default);

    Task UpdateAsync(int id, string name, string hexColor, CancellationToken cancellationToken = default);
}
