using Dotnetstore.JournalSystem.Shared.Constants;

namespace Dotnetstore.JournalSystem.Shared.Requests.Organization;

public sealed record CreateUserRequest(
    string FirstName,
    string? MiddleName,
    string LastName,
    string Username,
    Guid OwnCompanyId,
    Guid RoleId,
    AuthProvider AuthProvider,
    string? Password);
