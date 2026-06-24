using Microsoft.EntityFrameworkCore;
using TheDeskWatch.MobileApp.Contracts.Repositories;
using TheDeskWatch.Persistence.Data;

namespace TheDeskWatch.Persistence.Repositories;

internal sealed class DeskDepartureRepository(DeskWatchDbContext context) : IDeskDepartureRepository
{
    public async Task<IReadOnlyList<DeskDepartureRecord>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.DeskDepartures
            .OrderBy(d => d.DepartedAt)
            .Select(d => new DeskDepartureRecord(d.Id, d.ColleagueId, d.DepartedAt))
            .ToListAsync(cancellationToken);
}
