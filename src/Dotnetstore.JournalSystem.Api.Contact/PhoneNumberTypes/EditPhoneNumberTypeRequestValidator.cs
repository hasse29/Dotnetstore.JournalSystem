using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;

internal static class EditPhoneNumberTypeRequestValidator
{
    internal static Dictionary<string, string[]> Validate(EditPhoneNumberTypeRequest? request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request is null)
        {
            errors[nameof(request)] = ["A request body is required."];
            return errors;
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors[nameof(request.Name)] = [$"{nameof(request.Name)} is required."];
        }
        else if (request.Name.Length > DataSchemeConstants.PhoneNumberTypeNameMaxLength)
        {
            errors[nameof(request.Name)] = [$"{nameof(request.Name)} must be at most {DataSchemeConstants.PhoneNumberTypeNameMaxLength} characters."];
        }

        if (request.Description is null)
        {
            errors[nameof(request.Description)] = [$"{nameof(request.Description)} is required."];
        }
        else if (request.Description.Length > DataSchemeConstants.PhoneNumberTypeDescriptionMaxLength)
        {
            errors[nameof(request.Description)] = [$"{nameof(request.Description)} must be at most {DataSchemeConstants.PhoneNumberTypeDescriptionMaxLength} characters."];
        }

        return errors;
    }
}
