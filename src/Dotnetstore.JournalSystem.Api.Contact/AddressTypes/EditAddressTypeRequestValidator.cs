using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.AddressTypes;

internal static class EditAddressTypeRequestValidator
{
    internal static Dictionary<string, string[]> Validate(EditAddressTypeRequest? request)
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
        else if (request.Name.Length > DataSchemeConstants.AddressTypeNameMaxLength)
        {
            errors[nameof(request.Name)] = [$"{nameof(request.Name)} must be at most {DataSchemeConstants.AddressTypeNameMaxLength} characters."];
        }

        if (request.Description is null)
        {
            errors[nameof(request.Description)] = [$"{nameof(request.Description)} is required."];
        }
        else if (request.Description.Length > DataSchemeConstants.AddressTypeDescriptionMaxLength)
        {
            errors[nameof(request.Description)] = [$"{nameof(request.Description)} must be at most {DataSchemeConstants.AddressTypeDescriptionMaxLength} characters."];
        }

        return errors;
    }
}
