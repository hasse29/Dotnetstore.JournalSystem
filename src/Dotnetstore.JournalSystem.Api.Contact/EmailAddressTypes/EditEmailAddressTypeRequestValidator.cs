using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;

internal static class EditEmailAddressTypeRequestValidator
{
    internal static Dictionary<string, string[]> Validate(EditEmailAddressTypeRequest? request)
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
        else if (request.Name.Length > DataSchemeConstants.EmailAddressTypeNameMaxLength)
        {
            errors[nameof(request.Name)] = [$"{nameof(request.Name)} must be at most {DataSchemeConstants.EmailAddressTypeNameMaxLength} characters."];
        }

        if (request.Description is null)
        {
            errors[nameof(request.Description)] = [$"{nameof(request.Description)} is required."];
        }
        else if (request.Description.Length > DataSchemeConstants.EmailAddressTypeDescriptionMaxLength)
        {
            errors[nameof(request.Description)] = [$"{nameof(request.Description)} must be at most {DataSchemeConstants.EmailAddressTypeDescriptionMaxLength} characters."];
        }

        return errors;
    }
}
