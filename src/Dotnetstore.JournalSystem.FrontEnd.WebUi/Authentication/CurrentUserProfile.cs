using System.Security.Claims;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.Extensions.Caching.Memory;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;

public interface ICurrentUserProfile
{
    /// <summary>The signed-in person's profile, or null when it cannot be loaded. Callers fall back to the sign-in name.</summary>
    Task<MyProfileResponse?> GetAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
}

/// <summary>
/// Loads who the signed-in person is from the API for both password and Microsoft sign-in. Every page shows the name in
/// the top bar, so a successful answer is kept briefly per person instead of calling the API on each request.
/// </summary>
internal sealed class CurrentUserProfile(IProfileClient profiles, IMemoryCache cache) : ICurrentUserProfile
{
    internal static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    public async Task<MyProfileResponse?> GetAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var key = CacheKey(principal);
        if (key is null)
        {
            return null;
        }

        if (cache.TryGetValue(key, out MyProfileResponse? cached))
        {
            return cached;
        }

        try
        {
            var result = await profiles.GetMineAsync(cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess || result.Value is null)
            {
                return null;
            }

            cache.Set(key, result.Value, CacheDuration);
            return result.Value;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TimeoutException)
        {
            return null;
        }
    }

    internal static string? CacheKey(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var id = principal.FindFirst(AuthClaimTypes.UserId)?.Value
                 ?? principal.FindFirst("oid")?.Value
                 ?? principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                 ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return id is null ? null : $"profile:{id}";
    }
}
