using Microsoft.EntityFrameworkCore;
using TheDeskWatch.MobileApp.Contracts.Repositories;
using TheDeskWatch.Persistence.Data;

namespace TheDeskWatch.Persistence.Repositories;

internal sealed class ColleagueRepository(DeskWatchDbContext context) : IColleagueRepository
{
    public async Task<IReadOnlyList<ColleagueRecord>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Colleagues
            .Select(c => new ColleagueRecord(c.Name, c.HexColor))
            .ToListAsync(cancellationToken);
}
