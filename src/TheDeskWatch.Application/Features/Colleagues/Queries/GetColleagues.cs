using LiteBus.Queries.Abstractions;
using OneOf;
using TheDeskWatch.Application.Common;

namespace TheDeskWatch.Application.Features.Colleagues.Queries;

public record GetColleaguesQuery : IQuery<OneOf<GetColleaguesResponse, ApiError>>;

public sealed record GetColleaguesResponse(IReadOnlyList<GetColleaguesResponse.ColleagueDto> Colleagues)
{
    public sealed record ColleagueDto(string Name, string HexColor);
}

public sealed class GetColleaguesQueryHandler
    : IQueryHandler<GetColleaguesQuery, OneOf<GetColleaguesResponse, ApiError>>
{
    public Task<OneOf<GetColleaguesResponse, ApiError>> HandleAsync(
        GetColleaguesQuery query,
        CancellationToken cancellationToken = new())
    {
        var response = new GetColleaguesResponse(
        [
            new GetColleaguesResponse.ColleagueDto("Alice Martin", "#E74C3C"),
            new GetColleaguesResponse.ColleagueDto("Bob Leclerc", "#3498DB"),
            new GetColleaguesResponse.ColleagueDto("Clara Dubois", "#2ECC71"),
            new GetColleaguesResponse.ColleagueDto("David Moreau", "#9B59B6"),
            new GetColleaguesResponse.ColleagueDto("Emma Bernard", "#F39C12"),
            new GetColleaguesResponse.ColleagueDto("Florian Rousseau", "#1ABC9C"),
            new GetColleaguesResponse.ColleagueDto("Gaëlle Simon", "#E67E22"),
            new GetColleaguesResponse.ColleagueDto("Hugo Laurent", "#2980B9"),
        ]);

        return Task.FromResult<OneOf<GetColleaguesResponse, ApiError>>(response);
    }
}
