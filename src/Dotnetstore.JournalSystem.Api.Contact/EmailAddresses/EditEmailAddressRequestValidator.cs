using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;

internal static class EditEmailAddressRequestValidator
{
    internal static Dictionary<string, string[]> Validate(EditEmailAddressRequest? request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request is null)
        {
            errors[nameof(request)] = ["A request body is required."];
            return errors;
        }

        AddEmptyGuidError(errors, nameof(request.EmailAddressTypeId), request.EmailAddressTypeId);
        AddEmptyGuidError(errors, nameof(request.ObjectId), request.ObjectId);

        if (string.IsNullOrWhiteSpace(request.Address))
        {
            errors[nameof(request.Address)] = [$"{nameof(request.Address)} is required."];
        }
        else if (request.Address.Length > DataSchemeConstants.EmailAddressMaxLength)
        {
            errors[nameof(request.Address)] = [$"{nameof(request.Address)} must be at most {DataSchemeConstants.EmailAddressMaxLength} characters."];
        }
        else if (!EmailAddressFormat.IsValid(request.Address))
        {
            errors[nameof(request.Address)] = [$"{nameof(request.Address)} must be a valid email address."];
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
