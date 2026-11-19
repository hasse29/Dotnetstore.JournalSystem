using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.Addresses;

internal static class EditAddressRequestValidator
{
    internal static Dictionary<string, string[]> Validate(EditAddressRequest? request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request is null)
        {
            errors[nameof(request)] = ["A request body is required."];
            return errors;
        }

        AddEmptyGuidError(errors, nameof(request.AddressTypeId), request.AddressTypeId);
        AddEmptyGuidError(errors, nameof(request.ObjectId), request.ObjectId);

        AddMaxLengthError(errors, nameof(request.CO), request.CO, DataSchemeConstants.AddressCOMaxLength);
        AddRequiredError(errors, nameof(request.Street1), request.Street1, DataSchemeConstants.AddressStreetMaxLength);
        AddMaxLengthError(errors, nameof(request.Street2), request.Street2, DataSchemeConstants.AddressStreetMaxLength);
        AddRequiredError(errors, nameof(request.PostalCode), request.PostalCode, DataSchemeConstants.AddressPostalCodeMaxLength);
        AddRequiredError(errors, nameof(request.City), request.City, DataSchemeConstants.AddressCityMaxLength);
        AddRequiredError(errors, nameof(request.Country), request.Country, DataSchemeConstants.AddressCountryMaxLength);

        if (request.State is null)
        {
            errors[nameof(request.State)] = [$"{nameof(request.State)} is required."];
        }
        else
        {
            AddMaxLengthError(errors, nameof(request.State), request.State, DataSchemeConstants.AddressStateMaxLength);
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

    private static void AddRequiredError(Dictionary<string, string[]> errors, string field, string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[field] = [$"{field} is required."];
            return;
        }

        AddMaxLengthError(errors, field, value, maxLength);
    }

    private static void AddMaxLengthError(Dictionary<string, string[]> errors, string field, string? value, int maxLength)
    {
        if (value is not null && value.Length > maxLength)
        {
            errors[field] = [$"{field} must be at most {maxLength} characters."];
        }
    }
}
