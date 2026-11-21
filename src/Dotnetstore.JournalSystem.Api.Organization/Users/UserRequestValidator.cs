using System.Net.Mail;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal static class UserRequestValidator
{
    internal static Dictionary<string, string[]> Validate(CreateUserRequest? request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request is null)
        {
            errors[nameof(request)] = ["A request body is required."];
            return errors;
        }

        ValidateNames(errors, request.FirstName, request.MiddleName, request.LastName);
        ValidateIds(errors, request.OwnCompanyId, request.RoleId);
        PersonDetailsValidator.Validate(errors, request.DateOfBirth, request.SocialSecurityNumber);

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            errors[nameof(request.Username)] = [$"{nameof(request.Username)} is required."];
        }
        else if (request.Username.Length > DataSchemeConstants.UserUsernameMaxLength ||
                 !MailAddress.TryCreate(request.Username, out var address) ||
                 address.Address != request.Username)
        {
            errors[nameof(request.Username)] = [$"{nameof(request.Username)} must be a valid email address of at most {DataSchemeConstants.UserUsernameMaxLength} characters."];
        }

        if (!Enum.IsDefined(request.AuthProvider))
        {
            errors[nameof(request.AuthProvider)] = [$"{nameof(request.AuthProvider)} is not valid."];
        }
        else if (request.AuthProvider == AuthProvider.Local)
        {
            if (string.IsNullOrEmpty(request.Password))
            {
                errors[nameof(request.Password)] = [$"{nameof(request.Password)} is required for local users."];
            }
            else if (request.Password.Length is < DataSchemeConstants.UserPasswordMinLength or > DataSchemeConstants.UserPasswordMaxLength)
            {
                errors[nameof(request.Password)] = [$"{nameof(request.Password)} must be between {DataSchemeConstants.UserPasswordMinLength} and {DataSchemeConstants.UserPasswordMaxLength} characters."];
            }
        }
        else if (request.Password is not null)
        {
            errors[nameof(request.Password)] = [$"{nameof(request.Password)} must not be set for Entra users."];
        }

        return errors;
    }

    internal static Dictionary<string, string[]> Validate(EditUserRequest? request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request is null)
        {
            errors[nameof(request)] = ["A request body is required."];
            return errors;
        }

        ValidateNames(errors, request.FirstName, request.MiddleName, request.LastName);
        ValidateIds(errors, request.OwnCompanyId, request.RoleId);
        PersonDetailsValidator.Validate(errors, request.DateOfBirth, request.SocialSecurityNumber);

        return errors;
    }

    internal static void ValidateNames(
        Dictionary<string, string[]> errors,
        string firstName,
        string? middleName,
        string lastName)
    {
        ValidateName(errors, nameof(CreateUserRequest.FirstName), firstName, required: true);
        ValidateName(errors, nameof(CreateUserRequest.MiddleName), middleName, required: false);
        ValidateName(errors, nameof(CreateUserRequest.LastName), lastName, required: true);
    }

    private static void ValidateName(Dictionary<string, string[]> errors, string field, string? value, bool required)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required || value is not null)
            {
                errors[field] = [$"{field} is required."];
            }
        }
        else if (value.Length > DataSchemeConstants.UserNameMaxLength)
        {
            errors[field] = [$"{field} must be at most {DataSchemeConstants.UserNameMaxLength} characters."];
        }
    }

    private static void ValidateIds(Dictionary<string, string[]> errors, Guid ownCompanyId, Guid roleId)
    {
        if (ownCompanyId == Guid.Empty)
        {
            errors[nameof(CreateUserRequest.OwnCompanyId)] = [$"{nameof(CreateUserRequest.OwnCompanyId)} is required."];
        }

        if (roleId == Guid.Empty)
        {
            errors[nameof(CreateUserRequest.RoleId)] = [$"{nameof(CreateUserRequest.RoleId)} is required."];
        }
    }
}
