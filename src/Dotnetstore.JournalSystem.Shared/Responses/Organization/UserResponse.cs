using Dotnetstore.JournalSystem.Shared.Constants;

namespace Dotnetstore.JournalSystem.Shared.Responses.Organization;

public sealed record UserResponse(
    Guid Id,
    string FirstName,
    string? MiddleName,
    string LastName,
    string Username,
    Guid OwnCompanyId,
    Guid RoleId,
    AuthProvider AuthProvider,
    bool IsLockedOut,
    DateTime? LastLoginAt,
    string? TemporaryPassword = null);
