using Ardalis.Result;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal sealed record EntraUserDetails(string FirstName, string LastName, string Username);

internal sealed record EntraUserCreated(string ExternalId, string? TemporaryPassword);

internal interface IEntraUserProvisioner
{
    ValueTask<Result<EntraUserCreated>> CreateAsync(EntraUserDetails details, CancellationToken cancellationToken = default);
    ValueTask<Result> UpdateAsync(string externalId, EntraUserDetails details, CancellationToken cancellationToken = default);
    ValueTask<Result> DeleteAsync(string externalId, CancellationToken cancellationToken = default);
}
