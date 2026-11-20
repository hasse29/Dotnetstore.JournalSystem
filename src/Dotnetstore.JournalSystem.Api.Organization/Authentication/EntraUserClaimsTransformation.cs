using System.Security.Claims;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnetstore.JournalSystem.Api.Organization.Authentication;

// The unit of work is resolved lazily: the authentication service constructs this class for every request,
// including health checks and anonymous calls that must work without a database.
internal sealed class EntraUserClaimsTransformation(IServiceProvider services) : IClaimsTransformation
{
    private const string ObjectIdClaim = "oid";
    private const string ObjectIdClaimLong = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    async Task<ClaimsPrincipal> IClaimsTransformation.TransformAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.Identity is not ClaimsIdentity { IsAuthenticated: true } identity ||
            identity.HasClaim(x => x.Type == AuthClaimTypes.UserId))
        {
            return principal;
        }

        var objectId = (principal.FindFirst(ObjectIdClaim) ?? principal.FindFirst(ObjectIdClaimLong))?.Value;
        if (objectId is null)
        {
            return principal;
        }

        var transformed = new ClaimsIdentity(identity);
        foreach (var claim in transformed.FindAll(AuthClaimTypes.Role).ToList())
        {
            transformed.RemoveClaim(claim);
        }

        var organizationUnitOfWork = services.GetRequiredService<IOrganizationUnitOfWork>();

        var user = await organizationUnitOfWork
            .Users
            .GetByExternalIdAsync(objectId)
            .ConfigureAwait(false);

        if (user is null || !user.AccountIsApproved || user.IsLockedOut)
        {
            return new ClaimsPrincipal(transformed);
        }

        var role = await organizationUnitOfWork
            .Roles
            .GetByIdAsync(user.RoleId)
            .ConfigureAwait(false);

        transformed.AddClaim(new Claim(AuthClaimTypes.UserId, user.Id.ToString()));
        transformed.AddClaim(new Claim(AuthClaimTypes.OwnCompanyId, user.OwnCompanyId.ToString()));
        if (role is not null)
        {
            transformed.AddClaim(new Claim(AuthClaimTypes.Role, role.Name));
        }

        return new ClaimsPrincipal(transformed);
    }
}
