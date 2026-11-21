using Dotnetstore.JournalSystem.Shared.Constants;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal static class PersonDetailsValidator
{
    private static readonly DateOnly EarliestBirthDate = new(1900, 1, 1);

    internal static void Validate(
        Dictionary<string, string[]> errors,
        DateOnly? dateOfBirth,
        string? personalIdentityNumber)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (dateOfBirth is { } date &&
            (date < EarliestBirthDate || date > DateOnly.FromDateTime(DateTime.UtcNow)))
        {
            errors["DateOfBirth"] = ["DateOfBirth must be a date between 1900-01-01 and today."];
        }

        // An empty string is allowed: on edit it means "clear the number".
        if (!string.IsNullOrEmpty(personalIdentityNumber) && !PersonalIdentityNumber.IsValid(personalIdentityNumber))
        {
            errors["SocialSecurityNumber"] =
            [
                $"SocialSecurityNumber must be 10 or 12 digits (up to {DataSchemeConstants.PersonalIdentityNumberMaxLength} without separators)."
            ];
        }
    }
}
