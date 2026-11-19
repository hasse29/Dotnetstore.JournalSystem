namespace Dotnetstore.JournalSystem.Shared.Responses.Organization;

public sealed record WorkGroupResponse(
    Guid Id,
    string Name,
    string Description);
