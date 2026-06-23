using TheDeskWatch.Application.Features.Colleagues.Commands;
using TheDeskWatch.MobileApp.Contracts.Repositories;

namespace TheDeskWatch.Application.Tests.Features.Colleagues.Commands;

public class AddColleagueCommandTests
{
    [Fact]
    public async Task HandleAsync_AddsColleagueToRepository()
    {
        var repository = new RecordingColleagueRepository();
        var handler = new AddColleagueCommandHandler(repository);

        var result = await handler.HandleAsync(new AddColleagueCommand("Emma de Vries", "#E74C3C"));

        Assert.True(result.IsT0);
        Assert.Equal(("Emma de Vries", "#E74C3C"), repository.Added.Single());
    }

    [Fact]
    public async Task HandleAsync_ReturnsApiError_WhenRepositoryThrows()
    {
        var repository = new ThrowingColleagueRepository();
        var handler = new AddColleagueCommandHandler(repository);

        var result = await handler.HandleAsync(new AddColleagueCommand("Emma de Vries", "#E74C3C"));

        Assert.True(result.IsT1);
    }

    private sealed class RecordingColleagueRepository : IColleagueRepository
    {
        public List<(string Name, string HexColor)> Added { get; } = [];

        public Task<IReadOnlyList<ColleagueRecord>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ColleagueRecord>>([]);

        public Task AddAsync(string name, string hexColor, CancellationToken cancellationToken = default)
        {
            Added.Add((name, hexColor));
            return Task.CompletedTask;
        }

        public Task UpdateAsync(int id, string name, string hexColor, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class ThrowingColleagueRepository : IColleagueRepository
    {
        public Task<IReadOnlyList<ColleagueRecord>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ColleagueRecord>>([]);

        public Task AddAsync(string name, string hexColor, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Repository failure.");

        public Task UpdateAsync(int id, string name, string hexColor, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Repository failure.");
    }
}
