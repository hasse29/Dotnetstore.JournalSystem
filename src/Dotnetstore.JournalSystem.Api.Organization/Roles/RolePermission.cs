namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

/// <summary>One right a role holds; see the Permissions constants for the values.</summary>
internal sealed class RolePermission(Guid roleId, string permission)
{
    public Guid RoleId { get; private set; } = roleId;

    public string Permission { get; private set; } = permission;
}
