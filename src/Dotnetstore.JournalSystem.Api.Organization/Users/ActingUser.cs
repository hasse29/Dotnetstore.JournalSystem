using System.Security.Claims;
using Dotnetstore.JournalSystem.Shared.Constants;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

/// <summary>The rights of the person making a request.</summary>
internal sealed record ActingUser(bool IsAdministrator, IReadOnlySet<string> Permissions)
{
    internal static ActingUser From(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return new ActingUser(
            principal.HasClaim(AuthClaimTypes.Role, RoleNames.Administrator),
            principal.FindAll(AuthClaimTypes.Permission).Select(x => x.Value).ToHashSet(StringComparer.Ordinal));
    }
}
