namespace Dotnetstore.JournalSystem.Shared.Responses.Organization;

public sealed record RoleResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem = false);
