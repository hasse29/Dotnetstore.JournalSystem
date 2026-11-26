using Dotnetstore.JournalSystem.Api.Organization.Services;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal interface IUserAdministrationGuard
{
    /// <summary>Why the person may not give someone this role, or null when they may.</summary>
    Task<string?> CheckAssignmentAsync(ActingUser actor, Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Why the person may not change or delete this user, or null when they may. Pass the role the user is to have
    /// afterwards, if it is being changed.
    /// </summary>
    Task<string?> CheckChangeAsync(ActingUser actor, Guid userId, Guid? newRoleId, CancellationToken cancellationToken = default);
}

/// <summary>Stops someone who manages users from using that to gain, or hand out, more than they have.</summary>
internal sealed class UserAdministrationGuard(IOrganizationUnitOfWork organizationUnitOfWork) : IUserAdministrationGuard
{
    internal const string AdministratorRoleRefused = "Only administrators can give someone the Administrator role.";
    internal const string MoreRightsRefused = "You cannot give someone a role with rights that you do not have.";
    internal const string AdministratorUserRefused = "Only administrators can change or delete an administrator.";

    async Task<string?> IUserAdministrationGuard.CheckAssignmentAsync(
        ActingUser actor,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actor);

        return actor.IsAdministrator ? null : await CheckRoleAsync(actor, roleId, cancellationToken).ConfigureAwait(false);
    }

    async Task<string?> IUserAdministrationGuard.CheckChangeAsync(
        ActingUser actor,
        Guid userId,
        Guid? newRoleId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actor);

        if (actor.IsAdministrator)
        {
            return null;
        }

        var user = await organizationUnitOfWork.Users.FindUntrackedAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            // Nothing to protect; the request then answers that there is no such user.
            return null;
        }

        var current = await organizationUnitOfWork.Roles.GetAccessAsync(user.RoleId, cancellationToken).ConfigureAwait(false);
        if (current is { IsAdministrator: true })
        {
            return AdministratorUserRefused;
        }

        return newRoleId is { } roleId && roleId != user.RoleId
            ? await CheckRoleAsync(actor, roleId, cancellationToken).ConfigureAwait(false)
            : null;
    }

    private async Task<string?> CheckRoleAsync(ActingUser actor, Guid roleId, CancellationToken cancellationToken)
    {
        // An unknown role is left for validation to report.
        var role = await organizationUnitOfWork.Roles.GetAccessAsync(roleId, cancellationToken).ConfigureAwait(false);
        if (role is null)
        {
            return null;
        }

        if (role.IsAdministrator)
        {
            return AdministratorRoleRefused;
        }

        return role.Permissions.All(actor.Permissions.Contains) ? null : MoreRightsRefused;
    }
}
