namespace Dotnetstore.JournalSystem.Shared.Requests.Organization;

/// <param name="DateOfBirth">Left unchanged when null.</param>
/// <param name="SocialSecurityNumber">Left unchanged when null; an empty string clears it.</param>
/// <param name="IsMale">Left unchanged when null.</param>
public sealed record EditUserRequest(
    string FirstName,
    string? MiddleName,
    string LastName,
    Guid OwnCompanyId,
    Guid RoleId,
    DateOnly? DateOfBirth = null,
    string? SocialSecurityNumber = null,
    bool? IsMale = null);
