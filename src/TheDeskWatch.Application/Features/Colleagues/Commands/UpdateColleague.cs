using LiteBus.Commands.Abstractions;
using OneOf;
using TheDeskWatch.Application.Common;
using TheDeskWatch.MobileApp.Contracts.Repositories;

namespace TheDeskWatch.Application.Features.Colleagues.Commands;

public sealed record UpdateColleagueCommand(int Id, string Name, string HexColor)
    : ICommand<OneOf<UpdateColleagueResponse, ApiError>>;

public sealed record UpdateColleagueResponse;

internal sealed class UpdateColleagueCommandHandler(IColleagueRepository repository)
    : ICommandHandler<UpdateColleagueCommand, OneOf<UpdateColleagueResponse, ApiError>>
{
    public async Task<OneOf<UpdateColleagueResponse, ApiError>> HandleAsync(
        UpdateColleagueCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await repository.UpdateAsync(command.Id, command.Name, command.HexColor, cancellationToken);

            return new UpdateColleagueResponse();
        }
        catch (Exception exception)
        {
            return new ApiError(exception.Message);
        }
    }
}
