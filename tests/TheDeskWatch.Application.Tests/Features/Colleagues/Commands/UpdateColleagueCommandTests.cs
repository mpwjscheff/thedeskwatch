using TheDeskWatch.Application.Features.Colleagues.Commands;
using TheDeskWatch.MobileApp.Contracts.Repositories;

namespace TheDeskWatch.Application.Tests.Features.Colleagues.Commands;

public class UpdateColleagueCommandTests
{
    [Fact]
    public async Task HandleAsync_UpdatesColleagueInRepository()
    {
        var repository = new RecordingColleagueRepository();
        var handler = new UpdateColleagueCommandHandler(repository);

        var result = await handler.HandleAsync(new UpdateColleagueCommand(7, "Emma de Vries", "#E74C3C"));

        Assert.True(result.IsT0);
        Assert.Equal((7, "Emma de Vries", "#E74C3C"), repository.Updated.Single());
    }

    [Fact]
    public async Task HandleAsync_ReturnsApiError_WhenRepositoryThrows()
    {
        var repository = new ThrowingColleagueRepository();
        var handler = new UpdateColleagueCommandHandler(repository);

        var result = await handler.HandleAsync(new UpdateColleagueCommand(7, "Emma de Vries", "#E74C3C"));

        Assert.True(result.IsT1);
    }

    private sealed class RecordingColleagueRepository : IColleagueRepository
    {
        public List<(int Id, string Name, string HexColor)> Updated { get; } = [];

        public Task<IReadOnlyList<ColleagueRecord>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ColleagueRecord>>([]);

        public Task AddAsync(string name, string hexColor, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(int id, string name, string hexColor, CancellationToken cancellationToken = default)
        {
            Updated.Add((id, name, hexColor));
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingColleagueRepository : IColleagueRepository
    {
        public Task<IReadOnlyList<ColleagueRecord>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ColleagueRecord>>([]);

        public Task AddAsync(string name, string hexColor, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(int id, string name, string hexColor, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Repository failure.");
    }
}
