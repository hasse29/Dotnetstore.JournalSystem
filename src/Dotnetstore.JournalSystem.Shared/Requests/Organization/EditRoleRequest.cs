namespace Dotnetstore.JournalSystem.Shared.Requests.Organization;

public sealed record EditRoleRequest(
    string Name,
    string Description);
