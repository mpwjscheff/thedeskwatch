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
            new GetColleaguesResponse.ColleagueDto("Emma de Vries", "#E74C3C"),
            new GetColleaguesResponse.ColleagueDto("Liam van den Berg", "#3498DB"),
            new GetColleaguesResponse.ColleagueDto("Sophie Jansen", "#2ECC71"),
            new GetColleaguesResponse.ColleagueDto("Noah de Boer", "#9B59B6"),
            new GetColleaguesResponse.ColleagueDto("Julia Bakker", "#F39C12"),
            new GetColleaguesResponse.ColleagueDto("Finn van Dijk", "#1ABC9C"),
            new GetColleaguesResponse.ColleagueDto("Mila Visser", "#E67E22"),
            new GetColleaguesResponse.ColleagueDto("Lars Smit", "#2980B9"),
            new GetColleaguesResponse.ColleagueDto("Saar Meijer", "#E91E63"),
            new GetColleaguesResponse.ColleagueDto("Bram Peters", "#00BCD4"),
            new GetColleaguesResponse.ColleagueDto("Fleur Hendriks", "#8BC34A"),
            new GetColleaguesResponse.ColleagueDto("Thijs van der Laan", "#FF5722"),
        ]);

        return Task.FromResult<OneOf<GetColleaguesResponse, ApiError>>(response);
    }
}
