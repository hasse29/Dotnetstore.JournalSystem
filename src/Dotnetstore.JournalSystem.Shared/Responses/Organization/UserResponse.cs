using Dotnetstore.JournalSystem.Shared.Constants;

namespace Dotnetstore.JournalSystem.Shared.Responses.Organization;

/// <param name="SocialSecurityNumberMasked">Never the full number, for example 19900101-****.</param>
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
    string? TemporaryPassword = null,
    DateOnly? DateOfBirth = null,
    bool IsMale = false,
    string? SocialSecurityNumberMasked = null);
