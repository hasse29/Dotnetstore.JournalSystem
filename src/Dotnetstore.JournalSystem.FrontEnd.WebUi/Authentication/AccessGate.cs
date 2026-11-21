using System.Security.Claims;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;

internal enum AccessGateOutcome
{
    Allow,
    GoToStatus,
    SignOut
}

/// <summary>
/// Decides whether a signed-in session may use the application. Microsoft sign-in proves who someone is, not that
/// they have been let in: until an administrator approves the request they only get the status page.
/// </summary>
internal static class AccessGate
{
    private static readonly string[] ExemptPrefixes = ["/Account", "/Access", "/Error"];

    internal static bool IsExempt(string? viewEnginePath) =>
        viewEnginePath is not null &&
        ExemptPrefixes.Any(prefix => viewEnginePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    // Password sign-in already carries the application's own user id; only Microsoft sign-in sessions lack it.
    internal static bool HasApplicationAccount(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.HasClaim(claim => claim.Type == AuthClaimTypes.UserId);
    }

    internal static bool IsAdministratorOrUnknown(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.HasClaim(AuthClaimTypes.Role, RoleNames.Administrator) || !HasApplicationAccount(principal);
    }

    internal static AccessGateOutcome Decide(ApiResult<AccessRequestResponse> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsUnauthorized)
        {
            return AccessGateOutcome.SignOut;
        }

        // The API only answers 403 when the token has no Microsoft identity at all, so there is nothing to approve.
        if (result.IsForbidden)
        {
            return AccessGateOutcome.Allow;
        }

        return result is { IsSuccess: true, Value.Status: AccessRequestStatus.Approved }
            ? AccessGateOutcome.Allow
            : AccessGateOutcome.GoToStatus;
    }
}
