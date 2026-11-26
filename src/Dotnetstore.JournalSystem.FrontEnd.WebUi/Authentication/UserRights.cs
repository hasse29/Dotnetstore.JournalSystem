using System.Security.Claims;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;

/// <summary>
/// What the signed-in person may do, as the API says. Only used to decide what to show: the API checks every request.
/// </summary>
public sealed class UserRights
{
    private readonly HashSet<string> _permissions;

    private UserRights(bool isAdministrator, IEnumerable<string> permissions)
    {
        IsAdministrator = isAdministrator;
        _permissions = new HashSet<string>(permissions, StringComparer.Ordinal);
    }

    public static UserRights None { get; } = new(false, []);

    public bool IsAdministrator { get; }

    public bool Has(string permission) => IsAdministrator || _permissions.Contains(permission);

    /// <summary>No profile means no rights.</summary>
    public static UserRights From(MyProfileResponse? profile) =>
        profile is null ? None : new UserRights(profile.IsAdministrator, profile.Permissions ?? []);
}

public static class CurrentUserProfileExtensions
{
    public static async Task<UserRights> GetRightsAsync(
        this ICurrentUserProfile profiles,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(profiles);

        return UserRights.From(await profiles.GetAsync(principal, cancellationToken).ConfigureAwait(false));
    }
}
