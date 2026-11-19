using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;

internal static class OwnCompanyMappers
{
    internal static OwnCompanyResponse ToOwnCompanyResponse(this OwnCompany ownCompany)
    {
        return new OwnCompanyResponse(
            ownCompany.Id,
            ownCompany.Name,
            ownCompany.Description,
            ownCompany.LogoUrl,
            ownCompany.WebsiteUrl,
            ownCompany.CorporateNumber,
            ownCompany.TaxId);
    }
}