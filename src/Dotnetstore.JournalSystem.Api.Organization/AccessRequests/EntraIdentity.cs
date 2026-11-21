using System.Security.Claims;

namespace Dotnetstore.JournalSystem.Api.Organization.AccessRequests;

internal sealed record EntraIdentity(string ExternalId, string DisplayName, string Username)
{
    private const string ObjectIdClaim = "oid";
    private const string ObjectIdClaimLong = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    internal static EntraIdentity? From(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var objectId = (principal.FindFirst(ObjectIdClaim) ?? principal.FindFirst(ObjectIdClaimLong))?.Value;
        if (string.IsNullOrWhiteSpace(objectId))
        {
            return null;
        }

        var username = First(principal, "preferred_username", "upn", ClaimTypes.Upn, ClaimTypes.Email, "email") ?? objectId;
        var displayName = First(principal, "name", ClaimTypes.Name) ?? username;

        return new EntraIdentity(objectId, displayName, username);
    }

    private static string? First(ClaimsPrincipal principal, params string[] types)
    {
        foreach (var type in types)
        {
            var value = principal.FindFirst(type)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }
}
