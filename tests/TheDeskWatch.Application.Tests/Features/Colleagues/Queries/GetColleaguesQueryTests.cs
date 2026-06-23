using TheDeskWatch.Application.Features.Colleagues.Queries;

namespace TheDeskWatch.Application.Tests.Features.Colleagues.Queries;

public class GetColleaguesQueryTests
{
    [Fact]
    public async Task HandleAsync_ReturnsColleagues()
    {
        var handler = new GetColleaguesQueryHandler();

        var result = await handler.HandleAsync(new GetColleaguesQuery());

        Assert.True(result.IsT0);
        Assert.NotEmpty(result.AsT0.Colleagues);
    }

    [Fact]
    public async Task HandleAsync_EachColleagueHasNameAndHexColor()
    {
        var handler = new GetColleaguesQueryHandler();

        var result = await handler.HandleAsync(new GetColleaguesQuery());

        foreach (var colleague in result.AsT0.Colleagues)
        {
            Assert.False(string.IsNullOrWhiteSpace(colleague.Name));
            Assert.Matches("^#[0-9A-Fa-f]{6}$", colleague.HexColor);
        }
    }
}
