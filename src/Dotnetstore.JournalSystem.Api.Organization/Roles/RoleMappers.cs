using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

internal static class RoleMappers
{
    internal static RoleResponse ToRoleResponse(this Role role, IEnumerable<string>? permissions = null)
    {
        return new RoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.IsSystem,
            new RoleAccess(role.Name, [.. Shared.Constants.Permissions.WithRequired(permissions ?? [])]).EffectivePermissions);
    }
}
