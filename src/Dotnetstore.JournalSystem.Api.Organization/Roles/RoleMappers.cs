using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

internal static class RoleMappers
{
    internal static RoleResponse ToRoleResponse(this Role role)
    {
        return new RoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.IsSystem);
    }
}
