using System.Diagnostics.CodeAnalysis;
using Dotnetstore.BaseModel.Entities;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;

internal sealed class OwnCompany : Company
{
    [SetsRequiredMembers]
    internal OwnCompany(
        Guid id,
        string name,
        string? description,
        Uri? logoUrl,
        Uri? websiteUrl,
        string? corporateNumber,
        string? taxId)
    {
        Id = id;
        Name = name;
        Description = description;
        LogoUrl = logoUrl;
        WebsiteUrl = websiteUrl;
        CorporateNumber = corporateNumber;
        TaxId = taxId;
    }
}
