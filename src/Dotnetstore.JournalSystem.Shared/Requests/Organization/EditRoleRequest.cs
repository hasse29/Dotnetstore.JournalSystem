namespace Dotnetstore.JournalSystem.Shared.Requests.Organization;

/// <param name="Permissions">
/// The rights of the role. Left unchanged by an update when null; a new role then has none. Ignored for a system role.
/// </param>
public sealed record EditRoleRequest(
    string Name,
    string Description,
    IReadOnlyList<string>? Permissions = null);
