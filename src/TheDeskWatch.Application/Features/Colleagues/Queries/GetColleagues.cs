using LiteBus.Queries.Abstractions;
using OneOf;
using TheDeskWatch.Application.Common;
using TheDeskWatch.MobileApp.Contracts.Repositories;

namespace TheDeskWatch.Application.Features.Colleagues.Queries;

public record GetColleaguesQuery : IQuery<OneOf<GetColleaguesResponse, ApiError>>;

public sealed record GetColleaguesResponse(IReadOnlyList<GetColleaguesResponse.ColleagueDto> Colleagues)
{
    public sealed record ColleagueDto(string Name, string HexColor);
}

public sealed class GetColleaguesQueryHandler(IColleagueRepository repository)
    : IQueryHandler<GetColleaguesQuery, OneOf<GetColleaguesResponse, ApiError>>
{
    public async Task<OneOf<GetColleaguesResponse, ApiError>> HandleAsync(
        GetColleaguesQuery query,
        CancellationToken cancellationToken = new())
    {
        try
        {
            var colleagues = await repository.GetAllAsync(cancellationToken);

            var dtos = colleagues
                .Select(colleague => new GetColleaguesResponse.ColleagueDto(colleague.Name, colleague.HexColor))
                .ToList();

            return new GetColleaguesResponse(dtos);
        }
        catch (Exception exception)
        {
            return new ApiError(exception.Message);
        }
    }
}
