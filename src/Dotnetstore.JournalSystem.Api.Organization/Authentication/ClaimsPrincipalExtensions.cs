using System.Security.Claims;
using Dotnetstore.JournalSystem.Shared.Constants;

namespace Dotnetstore.JournalSystem.Api.Organization.Authentication;

internal static class ClaimsPrincipalExtensions
{
    internal static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var value = principal.FindFirst(AuthClaimTypes.UserId)?.Value;
        return Guid.TryParse(value, out var id) && id != Guid.Empty ? id : null;
    }
}
