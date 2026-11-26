using System.Security.Claims;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnetstore.JournalSystem.Api.Organization.Authentication;

/// <summary>
/// Gives each request the caller's role and rights as they are now, whichever way they signed in, so that a change
/// takes effect at once instead of when the token expires. Whatever a token says about roles or rights is discarded.
/// </summary>
// The unit of work is resolved lazily: the authentication service constructs this class for every request,
// including health checks and anonymous calls that must work without a database.
internal sealed class UserClaimsTransformation(IServiceProvider services) : IClaimsTransformation
{
    private const string ObjectIdClaim = "oid";
    private const string ObjectIdClaimLong = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    async Task<ClaimsPrincipal> IClaimsTransformation.TransformAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.Identity is not ClaimsIdentity { IsAuthenticated: true } identity || identity is ResolvedIdentity)
        {
            return principal;
        }

        // A password sign-in names the user in our own token; a Microsoft sign-in only names the person.
        var userId = identity.FindFirst(AuthClaimTypes.UserId)?.Value;
        var objectId = (identity.FindFirst(ObjectIdClaim) ?? identity.FindFirst(ObjectIdClaimLong))?.Value;

        var resolved = new ResolvedIdentity(identity);
        foreach (var claim in resolved.Claims
                     .Where(x => x.Type is AuthClaimTypes.Role or AuthClaimTypes.Permission or AuthClaimTypes.UserId or AuthClaimTypes.OwnCompanyId)
                     .ToList())
        {
            resolved.RemoveClaim(claim);
        }

        var user = userId is not null
            ? await FindPasswordUserAsync(userId).ConfigureAwait(false)
            : await FindMicrosoftUserAsync(objectId).ConfigureAwait(false);

        if (user is not null)
        {
            await AddClaimsAsync(resolved, user).ConfigureAwait(false);
        }

        return new ClaimsPrincipal(resolved);
    }

    // A lock after failed passwords does not end a session that is already open: anyone who knows a username could
    // otherwise sign that person out.
    private async Task<User?> FindPasswordUserAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var id))
        {
            return null;
        }

        var user = await UnitOfWork.Users.FindUntrackedAsync(id).ConfigureAwait(false);
        return user is { AccountIsApproved: true } ? user : null;
    }

    private async Task<User?> FindMicrosoftUserAsync(string? objectId)
    {
        if (objectId is null)
        {
            return null;
        }

        var user = await UnitOfWork.Users.GetByExternalIdAsync(objectId).ConfigureAwait(false);
        return user is { AccountIsApproved: true, IsLockedOut: false } ? user : null;
    }

    private async Task AddClaimsAsync(ClaimsIdentity identity, User user)
    {
        identity.AddClaim(new Claim(AuthClaimTypes.UserId, user.Id.ToString()));
        identity.AddClaim(new Claim(AuthClaimTypes.OwnCompanyId, user.OwnCompanyId.ToString()));

        var access = await UnitOfWork.Roles.GetAccessAsync(user.RoleId).ConfigureAwait(false);
        if (access is null)
        {
            return;
        }

        identity.AddClaim(new Claim(AuthClaimTypes.Role, access.Name));
        identity.AddClaims(access.EffectivePermissions.Select(x => new Claim(AuthClaimTypes.Permission, x)));
    }

    private IOrganizationUnitOfWork UnitOfWork => services.GetRequiredService<IOrganizationUnitOfWork>();

    // A type of its own, rather than a marker claim, so that no token can claim to be resolved already.
    private sealed class ResolvedIdentity(ClaimsIdentity identity) : ClaimsIdentity(identity, StringComparison.OrdinalIgnoreCase);
}
