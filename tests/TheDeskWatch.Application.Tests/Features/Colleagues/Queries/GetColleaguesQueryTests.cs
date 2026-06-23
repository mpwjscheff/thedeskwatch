using TheDeskWatch.Application.Features.Colleagues.Queries;
using TheDeskWatch.MobileApp.Contracts.Repositories;

namespace TheDeskWatch.Application.Tests.Features.Colleagues.Queries;

public class GetColleaguesQueryTests
{
    [Fact]
    public async Task HandleAsync_ReturnsColleagues()
    {
        var repository = new StubColleagueRepository(
        [
            new ColleagueRecord("Emma de Vries", "#E74C3C"),
            new ColleagueRecord("Liam van den Berg", "#3498DB"),
        ]);
        var handler = new GetColleaguesQueryHandler(repository);

        var result = await handler.HandleAsync(new GetColleaguesQuery());

        Assert.True(result.IsT0);
        Assert.NotEmpty(result.AsT0.Colleagues);
    }

    [Fact]
    public async Task HandleAsync_EachColleagueHasNameAndHexColor()
    {
        var repository = new StubColleagueRepository(
        [
            new ColleagueRecord("Emma de Vries", "#E74C3C"),
            new ColleagueRecord("Liam van den Berg", "#3498DB"),
        ]);
        var handler = new GetColleaguesQueryHandler(repository);

        var result = await handler.HandleAsync(new GetColleaguesQuery());

        foreach (var colleague in result.AsT0.Colleagues)
        {
            Assert.False(string.IsNullOrWhiteSpace(colleague.Name));
            Assert.Matches("^#[0-9A-Fa-f]{6}$", colleague.HexColor);
        }
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyList_WhenRepositoryReturnsNoColleagues()
    {
        var repository = new StubColleagueRepository([]);
        var handler = new GetColleaguesQueryHandler(repository);

        var result = await handler.HandleAsync(new GetColleaguesQuery());

        Assert.True(result.IsT0);
        Assert.Empty(result.AsT0.Colleagues);
    }

    [Fact]
    public async Task HandleAsync_ReturnsApiError_WhenRepositoryThrows()
    {
        var repository = new ThrowingColleagueRepository();
        var handler = new GetColleaguesQueryHandler(repository);

        var result = await handler.HandleAsync(new GetColleaguesQuery());

        Assert.True(result.IsT1);
    }

    private sealed class StubColleagueRepository(IReadOnlyList<ColleagueRecord> colleagues) : IColleagueRepository
    {
        public Task<IReadOnlyList<ColleagueRecord>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(colleagues);
    }

    private sealed class ThrowingColleagueRepository : IColleagueRepository
    {
        public Task<IReadOnlyList<ColleagueRecord>> GetAllAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Repository failure.");
    }
}
