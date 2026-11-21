namespace Dotnetstore.JournalSystem.Shared.Requests.Organization;

/// <summary>What the administrator fills in to turn an access request into a user.</summary>
public sealed record ApproveAccessRequestRequest(
    Guid OwnCompanyId,
    Guid RoleId,
    string FirstName,
    string? MiddleName,
    string LastName,
    bool IsMale,
    DateOnly? DateOfBirth = null,
    string? SocialSecurityNumber = null);
