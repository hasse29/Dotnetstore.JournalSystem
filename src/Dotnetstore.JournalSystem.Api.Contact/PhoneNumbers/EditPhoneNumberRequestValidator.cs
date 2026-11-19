using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;

internal static class EditPhoneNumberRequestValidator
{
    internal static Dictionary<string, string[]> Validate(EditPhoneNumberRequest? request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request is null)
        {
            errors[nameof(request)] = ["A request body is required."];
            return errors;
        }

        AddEmptyGuidError(errors, nameof(request.PhoneNumberTypeId), request.PhoneNumberTypeId);
        AddEmptyGuidError(errors, nameof(request.ObjectId), request.ObjectId);

        if (string.IsNullOrWhiteSpace(request.Number))
        {
            errors[nameof(request.Number)] = [$"{nameof(request.Number)} is required."];
        }
        else if (request.Number.Length > DataSchemeConstants.PhoneNumberMaxLength)
        {
            errors[nameof(request.Number)] = [$"{nameof(request.Number)} must be at most {DataSchemeConstants.PhoneNumberMaxLength} characters."];
        }
        else if (!PhoneNumberFormat.IsValid(request.Number))
        {
            errors[nameof(request.Number)] = [$"{nameof(request.Number)} must be a valid phone number."];
        }

        return errors;
    }

    private static void AddEmptyGuidError(Dictionary<string, string[]> errors, string field, Guid value)
    {
        if (value == Guid.Empty)
        {
            errors[field] = [$"{field} is required."];
        }
    }
}
