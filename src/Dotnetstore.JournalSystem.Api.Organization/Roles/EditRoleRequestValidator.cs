using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

internal static class EditRoleRequestValidator
{
    internal static Dictionary<string, string[]> Validate(EditRoleRequest? request)
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
        else if (request.Name.Length > DataSchemeConstants.RoleNameMaxLength)
        {
            errors[nameof(request.Name)] = [$"{nameof(request.Name)} must be at most {DataSchemeConstants.RoleNameMaxLength} characters."];
        }

        if (request.Description is null)
        {
            errors[nameof(request.Description)] = [$"{nameof(request.Description)} is required."];
        }
        else if (request.Description.Length > DataSchemeConstants.RoleDescriptionMaxLength)
        {
            errors[nameof(request.Description)] = [$"{nameof(request.Description)} must be at most {DataSchemeConstants.RoleDescriptionMaxLength} characters."];
        }

        return errors;
    }
}
