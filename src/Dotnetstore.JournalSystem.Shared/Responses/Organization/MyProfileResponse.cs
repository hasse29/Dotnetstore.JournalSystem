using Dotnetstore.JournalSystem.Shared.Constants;

namespace Dotnetstore.JournalSystem.Shared.Responses.Organization;

/// <summary>The signed-in person's own details, the same for password and Microsoft sign-in.</summary>
/// <param name="SocialSecurityNumberMasked">Never the full number, for example 19900101-****.</param>
public sealed record MyProfileResponse(
    Guid Id,
    string FirstName,
    string? MiddleName,
    string LastName,
    string Username,
    AuthProvider AuthProvider,
    DateOnly? DateOfBirth,
    bool IsMale,
    string? SocialSecurityNumberMasked,
    Guid OwnCompanyId,
    string OwnCompanyName,
    Guid RoleId,
    string RoleName,
    DateTime? LastLoginAt)
{
    public string FullName => string.Join(' ', new[] { FirstName, MiddleName, LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
}
