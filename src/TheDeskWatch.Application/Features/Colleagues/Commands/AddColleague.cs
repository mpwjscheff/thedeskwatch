using LiteBus.Commands.Abstractions;
using OneOf;
using TheDeskWatch.Application.Common;
using TheDeskWatch.MobileApp.Contracts.Repositories;

namespace TheDeskWatch.Application.Features.Colleagues.Commands;

public sealed record AddColleagueCommand(string Name, string HexColor)
    : ICommand<OneOf<AddColleagueResponse, ApiError>>;

public sealed record AddColleagueResponse;

internal sealed class AddColleagueCommandHandler(IColleagueRepository repository)
    : ICommandHandler<AddColleagueCommand, OneOf<AddColleagueResponse, ApiError>>
{
    public async Task<OneOf<AddColleagueResponse, ApiError>> HandleAsync(
        AddColleagueCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await repository.AddAsync(command.Name, command.HexColor, cancellationToken);

            return new AddColleagueResponse();
        }
        catch (Exception exception)
        {
            return new ApiError(exception.Message);
        }
    }
}
