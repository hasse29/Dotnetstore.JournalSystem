namespace Dotnetstore.JournalSystem.Shared.Requests.Organization;

public sealed record EditUserRequest(
    string FirstName,
    string? MiddleName,
    string LastName,
    Guid OwnCompanyId,
    Guid RoleId);
