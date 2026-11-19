using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;

internal static class EditOwnCompanyRequestValidator
{
    internal static Dictionary<string, string[]> Validate(EditOwnCompanyRequest? request)
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
        else if (request.Name.Length > DataSchemeConstants.OwnCompanyNameMaxLength)
        {
            errors[nameof(request.Name)] = [$"{nameof(request.Name)} must be at most {DataSchemeConstants.OwnCompanyNameMaxLength} characters."];
        }

        AddMaxLengthError(errors, nameof(request.Description), request.Description, DataSchemeConstants.OwnCompanyDescriptionMaxLength);
        AddMaxLengthError(errors, nameof(request.CorporateNumber), request.CorporateNumber, DataSchemeConstants.OwnCompanyCorporateNumberMaxLength);
        AddMaxLengthError(errors, nameof(request.TaxId), request.TaxId, DataSchemeConstants.OwnCompanyTaxIdMaxLength);

        return errors;
    }

    private static void AddMaxLengthError(Dictionary<string, string[]> errors, string field, string? value, int maxLength)
    {
        if (value is not null && value.Length > maxLength)
        {
            errors[field] = [$"{field} must be at most {maxLength} characters."];
        }
    }
}
