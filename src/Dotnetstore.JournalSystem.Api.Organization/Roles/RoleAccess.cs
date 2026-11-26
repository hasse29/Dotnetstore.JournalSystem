using Dotnetstore.JournalSystem.Shared.Constants;

namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

/// <summary>What a role gives the people who have it.</summary>
internal sealed record RoleAccess(string Name, IReadOnlyList<string> Permissions)
{
    // The administrator claim is the role's name, so the name decides here too.
    internal bool IsAdministrator => string.Equals(Name, RoleNames.Administrator, StringComparison.Ordinal);

    internal IReadOnlyList<string> EffectivePermissions => IsAdministrator ? Shared.Constants.Permissions.All : Permissions;
}
