using Microsoft.EntityFrameworkCore;
using TheDeskWatch.Domain;
using TheDeskWatch.MobileApp.Contracts.Repositories;
using TheDeskWatch.Persistence.Data;

namespace TheDeskWatch.Persistence.Repositories;

internal sealed class ColleagueRepository(DeskWatchDbContext context) : IColleagueRepository
{
    public async Task<IReadOnlyList<ColleagueRecord>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Colleagues
            .OrderBy(c => c.Name)
            .Select(c => new ColleagueRecord(c.Id, c.Name, c.HexColor))
            .ToListAsync(cancellationToken);

    public async Task AddAsync(string name, string hexColor, CancellationToken cancellationToken = default)
    {
        context.Colleagues.Add(new Colleague { Name = name, HexColor = hexColor });
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(int id, string name, string hexColor, CancellationToken cancellationToken = default)
    {
        var colleague = await context.Colleagues.FindAsync([id], cancellationToken)
            ?? throw new InvalidOperationException($"Colleague {id} not found.");
        colleague.Name = name;
        colleague.HexColor = hexColor;
        await context.SaveChangesAsync(cancellationToken);
    }
}
