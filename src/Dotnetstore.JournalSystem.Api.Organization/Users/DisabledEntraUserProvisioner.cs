using Ardalis.Result;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal sealed class DisabledEntraUserProvisioner : IEntraUserProvisioner
{
    private const string Message = "Entra ID user management is not configured.";

    ValueTask<Result<EntraUserCreated>> IEntraUserProvisioner.CreateAsync(EntraUserDetails details, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(Result<EntraUserCreated>.Unavailable(Message));
    }

    ValueTask<Result> IEntraUserProvisioner.UpdateAsync(string externalId, EntraUserDetails details, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(Result.Unavailable(Message));
    }

    ValueTask<Result> IEntraUserProvisioner.DeleteAsync(string externalId, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(Result.Unavailable(Message));
    }
}
